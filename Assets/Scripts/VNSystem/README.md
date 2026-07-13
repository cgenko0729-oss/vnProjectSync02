# VN System 使用说明

## 首次设置

1. 等待 Unity 完成脚本编译。
2. 打开要制作视觉小说的场景。
3. 执行 `Tools > VN System > Create or Repair Runtime Rig`。
4. 打开 `Assets/VNSystem/Data/VNContentCatalog.asset`。
5. 在 `characters` 中添加角色 ID、默认表情和各表情 Sprite。
6. 在 `backgrounds` 中添加背景 ID 与 Sprite。
7. 执行 `Tools > VN System > Validate Content Catalog`，直到 Console 无错误。

工具会创建 `VNRoot`、背景双缓冲层、五个角色槽位、镜头/天气/特效控制器，并在场景缺少时实例化 Pixel Crushers Dialogue Manager。它不会自动创建你的 Conversation 或修改现有 Dialogue Database。

## Dialogue System 命令

在 Dialogue Entry 的 `Sequence` 字段中使用：

```text
VNBG(bg.classroom.day, crossfade, 0.8, false)
VNChar(char.yuki, Center, smile, FadeSlideUp, 0.5, false)
VNFace(char.yuki, angry, 0.2, false)
VNMove(char.yuki, Left, 0.5, false)
VNHide(char.yuki, 0.4, false)
VNCamera(camera.push.soft, -1, false)
VNWeather(weather.rain.light, -1, false)
VNEffect(heartbeat, start, 0, false)
VNWait(0.4)
```

可用 `Tools > VN System > Sequence Command Builder` 生成并复制命令。命令结束前会阻塞当前 Sequence；持续天气和 `VNEffect(..., 0, ...)` 设置后立即继续。最后一个参数是 `instant`，读档或跳过时可直接应用最终状态。

## 自定义特效

在 `VNRoot` 的 `VNEffectDirector` 中增加 binding，例如 `heartbeat`。将 `onStart` 连接到 `VNHeartbeat.StartBeat()`，将 `onStop` 连接到 `VNHeartbeat.StopBeat()`。同样方式可接入现有的屏幕震动、景深、色调和粒子组件，不需要修改调度代码。

## 快照

```csharp
string json = VNDirector.Instance.CaptureSnapshot().ToJson();
VNDirector.Instance.RestoreSnapshot(VNStateSnapshot.FromJson(json));
```

快照保存背景、角色、表情、槽位、镜头预设、天气和持续特效。它只负责视觉状态；Dialogue System 的变量与 Conversation 状态仍应使用其 Save System 保存。

## 制作约束

- ID 使用稳定的小写命名，例如 `char.yuki`、`bg.classroom.day`。
- 简单演出使用上述命令；精确并行分镜使用 Dialogue System 的 `Timeline()` 命令。
- 不要让 Timeline 和 VN 命令在同一时段修改同一个镜头或角色属性。
- 提交资源时同时提交 `.meta` 文件，运行 Validator 后再提交剧情数据。
