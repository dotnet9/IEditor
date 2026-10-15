using System.Text.Json.Serialization;
using IEditor.App.Models;
namespace IEditor.App.Services;

/// <summary>偏好序列化元数据在编译时生成，支持 NativeAOT。</summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppPreferences))]
internal partial class PreferencesJsonContext : JsonSerializerContext;
