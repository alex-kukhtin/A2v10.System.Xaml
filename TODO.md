# TODO

No known defects open.

## Reviewed and rejected

From an outside review; recorded so they are not raised again.

- `Split`/`Contains` on names as a "hot path" — `Contains(char)` does not allocate; `Split` on short
  names is noise next to `XmlReader` and reflection. Not measured.
- Enum boxing in `PropertyConvertor.ConvertValue` — the result is `Object?` and goes to
  `PropertyInfo.SetValue`; boxing is unavoidable whatever `TryParse` is used.
- `Lazy<T>` in `XamlNode` — thread safety is indeed unneeded (~4 extra objects per node), but
  `IsValueCreated` carries meaning (`HasChildren`, `ProcessAttachedProperties`) and `Children` is a
  public mutable field; not worth the churn alone.
