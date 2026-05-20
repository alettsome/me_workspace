namespace me_workspace.Web.Features.Context;

public sealed record ContextSnapshotDto(IReadOnlyList<string> Sources, string Summary);
