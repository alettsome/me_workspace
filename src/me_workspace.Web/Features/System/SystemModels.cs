using me_workspace.Web.Features.Context;

namespace me_workspace.Web.Features.System;

public sealed record FeatureConnectionDto(string Name, string Status, string Detail);

public sealed record SystemFlowDto(
    string AppName,
    string Summary,
    ContextSnapshotDto Context,
    IReadOnlyList<FeatureConnectionDto> Connections);
