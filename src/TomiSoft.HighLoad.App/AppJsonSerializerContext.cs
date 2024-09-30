using System.Text.Json.Serialization;
using TomiSoft.HighLoad.App.Models.Api;

[JsonSerializable(typeof(SearchVehicleResultDto))]
[JsonSerializable(typeof(RegisteredVehicleDto))]
[JsonSerializable(typeof(RegisterVehicleRequestDto))]
[JsonSerializable(typeof(ErrorResponse))]
//[JsonSerializable(typeof(StringList))]
internal partial class AppJsonSerializerContext : JsonSerializerContext {

}

//internal partial class StringList : List<string> {
//}