# A2v10.System.Xaml

A standalone XAML reader/writer (author — Oleksandr Kukhtin), published as the
[A2v10.System.Xaml](https://www.nuget.org/packages/A2v10.System.Xaml) NuGet package. No WPF,
no Windows assemblies, no external dependencies. `README.md` is the public face of the
package; this file is for working inside the repository.

## Role in the platform

The package is consumed by `A2v10.Core` → `A2v10.ViewEngine.Xaml`, which loads every
`.vxaml` view through it. Two consumers, one code path: the running server renders the view,
and `a2 view validate` (the platform CLI) loads it the same way to report errors. Whatever
this reader fails to tell about an error, neither of them can tell either.

## How the reader is built

`XamlReader.Read()` runs in two phases, and most surprises come from this:

1. **Read** — `while (_rdr.Read())` turns the XML into a tree of `XamlNode`. Attributes are
   processed here, and `XamlNode.AddProperty` already resolves type descriptors — so some
   semantic errors are raised in this phase, while `XmlReader` still knows the position.
2. **Build** — `NodeBuilder.BuildNode(root)` walks that tree and instantiates objects.
   `XmlReader` is at end of file by now.

An error can therefore come from either phase, and the same kind of error (`Class X not
found`) can come from both, depending on whether the element carries attributes.

## Properties worth preserving

- **No `catch` anywhere in this repository.** Exceptions travel to the caller unwrapped —
  `XmlException` from malformed markup arrives with its own line info intact, and there is
  never a chain to dig through. If a `catch` becomes necessary, complete the exception and
  `throw;` rather than wrapping it: the CLI on the other end unwraps one level to pick the
  message, so an added layer silently changes what the user sees.
- **`XamlException` is the single error type of this reader.** One type to catch, one type to
  enrich.
- **A reader without line info stays valid.** `ParseXml(String)` and any injected `XmlReader`
  may report nothing; unknown position is a normal state, not a failure.

## Tests

```
dotnet test A2v10.System.Xaml.sln
```

Fixtures live in `A2v10.System.Xaml.Tests/TestFiles`. Coverage helpers: `codecoverage.ps1`,
`coverage.runsettings`.

## Open work

- [LINE-INFO.md](LINE-INFO.md) — carry the source line out of the reader (rethrowing located
  errors as `XmlException`) so that `a2 view validate` can point at the offending line. The
  consumer needs no change for it. Requested from the consumer side; not
  implemented yet. The point is not tidier output: that command is the feedback loop of an
  LLM writing views, and an error it cannot locate is an error the model tries to find by
  guessing — with edits landing in the wrong place and the same failure reported again.
