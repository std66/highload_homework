using System.Text.Json.Serialization;
using TomiSoft.HighLoad.App.Models.Api;

[JsonSerializable(typeof(SearchVehicleResultDto))]
[JsonSerializable(typeof(RegisteredVehicleDto))]
[JsonSerializable(typeof(RegisterVehicleRequestDto))]
[JsonSerializable(typeof(ErrorResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext {

}
