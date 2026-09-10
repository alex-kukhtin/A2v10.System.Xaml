# Line info in errors — what to add

A change request for this repository. Written from the consumer side (`A2v10.Core`), where
the gap shows up. Nothing here is implemented yet.

## Why

`a2 view validate` (the CLI of `A2v10.Core`) loads a `.vxaml` exactly as the server does and
reports the failure as JSON. It exists to be the **feedback loop of an LLM writing views**:
the model edits a file it cannot run, calls the tool, and acts on what comes back. The tool
is the ground truth — whatever it fails to state, the model has to invent.

That is what a missing line number costs. The message alone (`Property NoSuchProp not found
in type A2v10.Xaml.Text`) does not locate anything: the model has to go back to the file and
guess which occurrence is meant. In a real view — a grid with twenty `Text` elements, an
element repeated in three tabs — the guess is often wrong, and a wrong guess does not fail
loudly. It produces a confident edit in the wrong place, and the next validate run reports
the same error, so the model concludes its fix was insufficient and edits more. A tool that
cannot say *where* turns one broken attribute into a spreading diff.

A human reads these same errors while building a view, and pays the same cost, quietly.

There is a second, sharper reason. Right now malformed markup **does** report a line (that
comes free from `XmlException`) while semantic errors do not. So the consumer cannot build a
stable habit around the answer: sometimes it is told where to go, sometimes it must search,
and it cannot tell in advance which kind of answer it will get. Half a contract is worse than
none — the fix here is what makes `lineNo` mean the same thing in every failure.

Measured on four broken views:

| Broken thing | Exception | Position available |
| --- | --- | --- |
| malformed markup | `System.Xml.XmlException` | **yes** — `LineNumber`/`LinePosition` |
| `Class X not found` | `XamlException` | no |
| `Property X not found in type T` | `XamlException` | no |
| `Invalid enum value 'X' for 'Y'` | `XamlException` | no |

The first row already works end to end — the CLI reads `XmlException.LineNumber`. The other
three are the subject of this document.

## Why the position is lost

`XamlReader.Read()` works in two phases:

1. `while (_rdr.Read())` — the whole file is read into a tree of `XamlNode`.
2. `nodeBuilder.BuildNode(root)` — the tree is turned into objects.

`XmlReader` is the only thing that knows a position, and by phase 2 it is at end of file.
`XamlNode` does not record where it came from, so an error raised while building a node has
nothing to report.

One consequence worth knowing: `Class X not found` is raised from `GetNodeDescriptor`, which
is called **both** from `XamlNode.AddProperty` (phase 1, reader still standing on the element)
and from `BuildNode` (phase 2). So an unknown element *with* attributes fails while the
position is still live, and the same element *without* attributes fails after it is gone. The
fix must not rely on that difference.

## What to add

`XamlException` is **not** extended. A located error is rethrown as `System.Xml.XmlException`,
which already carries the position and which the consumer already understands. Verified:

```csharp
new XmlException("Property NoSuchProp not found in type A2v10.Xaml.Text", null, 6, 19)
// Message : Property NoSuchProp not found in type A2v10.Xaml.Text Line 6, position 19.
// LineNumber : 6   LinePosition : 19
```

The four-argument constructor is public, and the coordinates are appended to `Message` for
free — a person reading a terminal sees the line without any JSON field. Nothing in the
platform catches `XamlException` (checked across both repositories), so nothing breaks.

**1. `XamlNode` remembers its origin.** Two `Int32` fields, `Line` and `Position`, set in
`XamlReader.ReadNode` when the node is created:

```csharp
case XmlNodeType.Element:
    var node = new XamlNode(_rdr.Name);   // + line info from _rdr as IXmlLineInfo
```

Guard with `IXmlLineInfo.HasLineInfo()`; `0` means "unknown" and must stay a valid state —
`ParseXml(String)` and any reader without line info keep working exactly as now. When the
position is unknown, rethrow nothing: let the original `XamlException` travel as it does today.

**2. Rethrow with the position, at exactly two places.**

- Phase 1 — in `XamlReader.Read()`, around the read loop: `catch (XamlException ex)` → take
  the position from the reader (`_rdr as IXmlLineInfo`).
- Phase 2 — in `NodeBuilder.BuildNode(XamlNode node)`: `catch (XamlException ex)` → take
  `node.Line` / `node.Position`.

Both do the same thing:

```csharp
throw new XmlException(ex.Message, null, line, position);
```

**Pass `null` as the inner exception, not `ex`.** The consumer picks its message one level
down (`ex.InnerException ?? ex`), so an inner would send it back to the exception that has no
position — and `lineNo` would be `null` again. This is the one trap in the whole change.

No `when` guard is needed: once stamped, the exception is an `XmlException`, and the outer
`catch (XamlException)` frames no longer see it. The innermost node — the one that actually
failed — wins by construction, which is exactly what a deeply nested view needs.

The cost, stated plainly: the original stack of the throw is lost (irrelevant to the CLI, mildly
worse when debugging the reader itself), and semantic errors surface as `XmlException` rather
than `XamlException`. Both are acceptable only because no one catches either type today — if
that changes, revisit this.

## Tests

`A2v10.System.Xaml.Tests` already has the shape for this (`TestFiles`, `Types.cs`, `Enum.cs`).
Cases to add, each asserting a concrete line number:

- unknown class, element **with** attributes (fails in phase 1)
- unknown class, element **without** attributes (fails in phase 2) — same expected line
- unknown property
- invalid enum value
- a nested element failing several levels down — the reported line is the failing element, not
  the root
- a reader without line info — position stays `0`, nothing throws

## After it ships

In `A2v10.Core`: bump the `A2v10.System.Xaml` version in `A2v10.ViewEngine.Xaml.csproj`. That
is all — `Tools/A2v10.Cli/JsonResult.cs` already reads `XmlException.LineNumber`, so
`a2 view validate` starts reporting `lineNo` for semantic errors with no code change on that
side. Confirm with a broken view (unknown property inside a nested element) that the number
points at the element, not at the root.
