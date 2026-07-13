# Codex 工作记录与视觉小说通用系统方案

> 日期：2026-07-13（Asia/Tokyo）  
> 本文记录可复核的操作、发现、决策与后续方案；不包含模型内部思维链或不可验证的隐含推理。

## 1. 本次操作记录

1. 确认项目根目录原本没有 Git 仓库（无 `.git/`），因此当前还没有本地分支或提交历史可删除。
2. 检查 Git：已安装 `git 2.51.0.windows.2`。
3. 检查 GitHub CLI：系统找不到 `gh`。发布流程要求先安装并执行 `gh auth login`，所以本次没有初始化、提交或推送，避免留下未经确认的半完成状态。
4. 检查 `.gitignore`：已排除 `Library/`、`Temp/`、`obj/`、`Logs/`、`UserSettings/`、构建目录、IDE 文件和调试截图，基本符合 Unity 项目要求。
5. 检查待版本控制内容：排除生成目录后约 3,713 个文件、91.79 MB；最大单文件约 16.87 MB，未超过 GitHub 单文件 100 MB 限制。
6. 检查目标仓库 `cgenko0729-oss/vnProjectSync02`：使用系统 Git 成功连接；远端是空仓库，没有分支、标签或提交历史。
7. 通过 GitHub 公共 API 确认目标仓库的默认分支名为 `main`，且当前可见性为 **Public**。由于项目包含可能受再分发限制的付费插件源码，尚未推送。
8. 用户已明确授权不使用 `gh`，改用系统 Git / Windows Git Credential Manager（其底层能力与 SourceTree 相同）完成推送；此路径已验证能够访问远端。
9. 遵守用户要求：没有删除、重命名、强推或覆盖任何分支。
10. 用户随后明确说明：已联系插件作者并取得公开发布授权，要求忽略此前的许可证风险提示并继续上传。因此按用户确认，将完整项目发布到 Public 仓库；此授权确认作为本次发布决策依据记录在此。
11. 使用系统 Git 初始化本地仓库，初始分支为 `main`；添加远端 `origin=https://github.com/cgenko0729-oss/vnProjectSync02.git`。Git 提交身份为 `Genko1204019 <cgenko0729@gmail.com>`。
12. 首次暂存共 3,601 个文件；`Library/`、`Temp/`、`Logs/`、`obj/`、`UserSettings/`、`Assets/DebugScreenShot/`、生成的 `.csproj` 与 `.sln` 均保持忽略，没有加入版本控制。
13. 公开仓库敏感信息检查未发现私钥文件、常见 GitHub/API 令牌格式或敏感凭据文件；Git 对象总量约 42.97 MiB，无需 Git LFS。
14. 创建首次提交 `7138f6d`（`Initialize Unity visual novel project`），包含 3,601 个文件；创建注释标签 `baseline-before-vn-system`。
15. 使用普通、非强制推送成功发布 `main` 分支和 `baseline-before-vn-system` 标签；本地 `main` 已设置跟踪 `origin/main`。整个过程没有删除分支、没有强推、没有改写提交历史。

### GitHub 上传阻塞与下一步

当前无需安装 SourceTree 或 GitHub CLI；系统 Git 已可访问远端。虽然仓库是 Public，但用户已明确确认获得插件作者授权，并要求继续公开上传，因此不再将可见性作为阻塞条件。

如果以后希望使用 GitHub CLI，也可以安装后执行：

```powershell
gh auth login
gh auth status
```

获得授权确认后由 Codex 继续：`git init -b main` → 首次快照提交 → 添加 `origin` → 非强制推送。之后所有功能开发使用 `agent/功能名` 分支并保留历史。禁止使用 `git push --force`、`git reset --hard` 和任何删分支命令。重要里程碑额外打标签，例如 `vn-foundation-v0.1`，标签比长期堆积临时分支更适合“随时回到某版本”。

## 2. 上传前必须确认的授权风险

项目包含 `Assets/Plugins/Pixel Crushers/Dialogue System/`、DOTween、TextMesh Pro 及大量第三方演示资源。Pixel Crushers Dialogue System 可能属于付费 Asset Store 授权；将其源码上传到公开仓库可能构成再分发。首次推送前必须确认仓库为 **Private**，或根据各插件许可证排除不能再分发的文件，并为协作者提供合法的本地安装说明。不要在尚未确认授权时公开整个 `Assets/Plugins/`。

## 3. 当前项目审查结果

- Unity `6000.0.62f1`，URP `17.0.4`，Input System `1.14.2`，Unity Test Framework `1.6.0`。
- `SampleScene.unity` 是当前 Build Profiles 中唯一启用的场景。
- 项目已有约 31 个自研 `VNEffects` 脚本，覆盖镜头、转场、立绘入场、情绪、天气、屏幕震动、景深、色调、打字机和选项演出。
- Dialogue System 插件已完整安装，内含 Conversation 数据库、条件/变量、Sequencer、自定义命令模板、Timeline 与 Cinemachine 可选命令。
- 但项目自己的脚本、场景和 Prefab 中没有发现 Dialogue Manager、Dialogue Database 或 Dialogue System API 引用，说明插件目前尚未真正接入游戏。
- 尚无项目自有测试程序集。
- `GameIdea.txt` 与 `WhatAiDo.md` 在默认 PowerShell 编码下出现乱码显示风险；后续文本资产统一使用 UTF-8，并在提交前用编辑器实际确认。

## 4. 结论：使用混合架构

**推荐保留 Dialogue System，但只让它负责叙事层；新增轻量 VN Presentation 层负责演出。**

Dialogue System 适合管理：台词、说话人、条件、变量、选项、分支、跳转、本地化、会话状态、存档恢复。现有 VNEffects 适合管理：背景、立绘、表情、入退场、镜头、天气、转场和音画特效。两者之间使用少量自定义 Sequencer 命令连接。

不建议从零写完整引擎，因为分支条件、存档、回滚、本地化、编辑器、导入导出和边界情况的成本远高于“显示一句台词”。也不建议把所有演出都塞进 Dialogue System 的自由文本命令；字符串拼写错误难检查，复杂并行动画难预览。

## 5. 推荐架构

```text
Dialogue Database（剧情、分支、变量）
            |
            v
VNDialogueBridge（翻译 Sequencer 命令）
            |
            v
VNDirector（唯一调度器：等待、跳过、取消、并行、存档状态）
   |          |           |          |
Stage       Camera      Effects      Audio/UI
背景/立绘    镜头预设     天气/转场     对话/选项/声音
```

### 核心模块

- `VNDirector`：统一执行命令，维护当前演出句柄；支持串行、并行、等待、取消、快进和恢复。
- `VNStageController`：管理背景层、前景层和固定数量的角色槽位（FarLeft/Left/Center/Right/FarRight）。
- `VNCharacterView`：管理角色 Sprite、表情、姿势、朝向、亮暗、层级、入退场动画。
- `VNCameraDirector`：封装现有 `VNCamera`、`VNDutchAngle`、`VNScreenShake` 为命名预设。
- `VNEffectDirector`：封装天气、色调、转场、景深、心跳等，按通道替换或叠加。
- `VNDialogueBridge`：继承 Dialogue System 的 `SequencerCommand`，只暴露稳定、可验证的领域命令。
- `VNStateSnapshot`：保存逻辑状态与最终视觉状态，不保存正在运行的 Tween；读档时重建最终画面。

## 6. 数据与资源最佳实践

使用稳定 ID，不在剧情中直接引用场景对象名称或长资源路径：

- 角色：`char.yuki`
- 表情：`neutral`、`smile`、`angry`、`cry`
- 背景：`bg.classroom.day`
- 镜头预设：`cam.closeup.left`
- 演出预设：`enter.slide_left_soft`
- 天气：`weather.rain.light`

建立 `ScriptableObject` Catalog：`CharacterDefinition`、`BackgroundDefinition`、`CameraPreset`、`EffectPreset`。角色定义中保存显示名、各表情/姿势 Sprite、默认槽位、声音与演出预设。剧情数据库只保存 ID，运行时由 Catalog 解析。这样替换美术资源不需要逐条修改台词。

每个镜头节点建议包含：台词、说话人、角色焦点、前置条件、状态变更、进入命令、持续命令、退出命令。简单演出写命令；超过约 3 个并行动作或需要精确到帧的分镜，制作 Unity Timeline，并由 Dialogue System 的 `Timeline()` Sequencer 命令调用。

## 7. 建议的作者命令

不要为 31 个组件各建一种语法；先收敛为 8–12 个命令：

```text
VNBG(classroom_day, crossfade, 0.8)
VNChar(yuki, center, smile, enter.slide_left, 0.5)
VNFace(yuki, angry, dissolve, 0.2)
VNCamera(closeup_yuki, 1.2)
VNWeather(rain_light, fade, 1.0)
VNEffect(heartbeat, start)
VNWait(0.4)
VNTimeline(chapter01_shot12)
```

同一行默认并行，不同行按剧情节点顺序执行；需要阻塞时命令自身报告完成。所有命令都应支持 `instant`，用于读档、跳过和快速预览。编辑器要做参数下拉选择与 ID 校验，避免作者手打字符串。

## 8. Dialogue System 与轻量自研的取舍

| 方案 | 优点 | 缺点 | 结论 |
|---|---|---|---|
| 全用 Dialogue System | 分支、变量、存档和本地化成熟 | 演出文本容易膨胀，复杂并行不直观 | 不单独使用 |
| 全部自研 | API 最简、完全可控 | 要重造编辑器、分支、存档、导入导出和测试 | 当前不划算 |
| 混合方案 | 复用成熟叙事能力，保留现有效果代码 | 需要一层 Bridge 与规范 | **最推荐** |

如果最终游戏只有几十分钟、几乎没有分支与本地化，轻量 JSON/CSV + 自研播放器才可能更简单。你的需求同时包含分支、选择、复杂镜头和多类并行演出，已经超过这种轻量方案的舒适区。

## 9. 编辑工作流

1. 编剧在 Dialogue Editor 写 Conversation、条件、变量和选项。
2. 演出人员给节点选择背景、角色槽位、表情和演出预设。
3. 简单动作由 Sequencer 命令执行；复杂分镜进入 Timeline。
4. 点击“从当前节点预览”，系统自动搭建该节点的视觉状态。
5. Validator 检查缺失 ID、无入口节点、永远不可达分支、缺图、重复变量和无效命令。
6. 导出 CSV/表格给编剧与本地化；导入时以稳定 GUID/ID 合并，禁止按行号覆盖。

## 10. 实施路线图

### 阶段 0：版本控制与安全（0.5 天）

安装并登录 `gh`；确认仓库 Private；首次快照提交；打 `baseline-before-vn-system` 标签。此后每阶段独立分支，不删除旧分支，不强推。

### 阶段 1：最小纵向切片（2–4 天）

接入 Dialogue Manager 与一个 Dialogue Database；完成背景切换、单角色显示/表情、逐字文本、继续输入、两个选项和一个分支。先验证完整流程，不先做所有特效。

### 阶段 2：演出调度（3–5 天）

实现 `VNDirector`、取消令牌、并行组、等待策略和 `instant` 模式；把镜头、转场、天气、震动接入统一接口。杜绝不同 Tween 同时写同一个 Transform/Material 属性。

### 阶段 3：内容数据化（3–5 天）

制作 Character/Background/Camera/Effect Catalog 和自定义 Inspector；建立稳定 ID、角色槽位、预设与资源校验。

### 阶段 4：存档与体验（3–6 天）

加入存档/读档、自动播放、快进已读、历史记录、文字速度、音量、隐藏 UI。回滚需要记录每个已完成节点的逻辑变量和视觉快照；它比普通存档复杂，应单独实现和测试。

### 阶段 5：制作工具（持续）

节点预览、批量校验、CSV/Google Sheets 导入导出、本地化、资源使用报告和构建前检查。只在作者痛点真实出现后扩展工具，不要一次性造一个庞大编辑器。

## 11. 验收标准与测试

- 同一剧情从新游戏、读档、跳过三种路径到达同一节点时，画面与变量一致。
- 选项条件与分支结果可用 Edit Mode 测试验证。
- 所有命令在正常、快进、取消、场景卸载时都能结束，不遗留 Tween 或粒子。
- 缺失角色/背景/表情 ID 在编辑器中报错，而不是运行时静默失败。
- 50–100 次连续换背景和立绘不持续增长对象数或材质实例。
- 每个 PR 至少说明测试节点，并为 UI、镜头和特效变更附截图或录屏。

## 12. 下一步建议

先不要继续堆独立特效组件。下一项开发应是“阶段 1 最小纵向切片”：只选一段 10–20 句、2 个角色、2 个背景、1 次表情变化、1 次镜头移动和 1 个二选一分支。它会验证 Dialogue System 与现有 VNEffects 的边界，并为后续所有内容建立可复制模板。

## 13. 进一步技术规格与制作决策（2026-07-13）

### 插件与依赖确认

- 当前 Dialogue System for Unity 版本为 `2.2.73`。
- 插件自带 `SequencerCommandTemplate`，支持读取参数、等待异步演出、结束时 `Stop()`，并要求在取消时通过 `OnDestroy()` 清理状态。这正适合作为 `VNDirector` 的桥接入口。
- Unity Timeline `1.8.9` 已安装，可以承载复杂分镜。
- 当前未安装 Cinemachine。第一版继续使用现有 `VNCamera`；只有出现多虚拟机位混合、目标跟随、轨道镜头等明确需求时再引入 Cinemachine，避免同时维护两套镜头系统。

### 时间与并发语义

所有演出必须明确属于以下一种：

1. **立即命令**：设置表情、切换说话人高亮等，当帧完成。
2. **阻塞命令**：必须等转场、角色入场或镜头运动结束才显示下一句。
3. **非阻塞命令**：雨、呼吸、背景缓慢推镜等持续播放，台词可以继续。
4. **并行组**：例如背景淡出、立绘入场、镜头推进同时开始，并等待组内指定的最长动作。
5. **持久状态**：背景、角色槽位、天气、色调；读档时直接恢复最终状态，不重播入场动画。

每类资源通道只能有一个所有者：Camera 通道由 `VNCameraDirector` 写入，Character/Yuki 通道由对应 `VNCharacterView` 写入，Weather 通道由 `VNEffectDirector` 写入。新命令进入同一通道时采用明确策略：`Replace`、`Queue` 或 `Ignore`，不可让多个 DOTween 静默争写同一属性。

### 推荐的场景层级

```text
VNRoot
├─ BackgroundRoot（背景 A/B 双缓冲交叉淡化）
├─ StageRoot
│  ├─ SlotFarLeft
│  ├─ SlotLeft
│  ├─ SlotCenter
│  ├─ SlotRight
│  └─ SlotFarRight
├─ WeatherRoot（非 UI 世界/屏幕特效）
├─ CameraRig（现有 ZoomRoot/TiltRoot/SceneRoot）
├─ OverlayRoot（闪白、转场、色调、Vignette）
└─ DialogueCanvas（姓名、台词、历史、选项、系统菜单）
```

背景使用 A/B 双层而不是销毁再创建，以便稳定交叉淡化。角色槽位固定，但角色实例与槽位分离；同一角色可换位，槽位只表达构图。选项 UI 独立于普通台词 UI，并由 Dialogue System 的响应列表驱动，禁止再维护第二套分支数据。

### 对话节点的最小数据约定

每个 Dialogue Entry 只保存叙事事实与少量演出引用：

- `Actor` / `Conversant`：说话人与对象。
- `Dialogue Text`：台词正文。
- `Conditions`：出现条件。
- `User Script`：变量更新、好感度和剧情旗标。
- `Sequence`：调用命名演出命令或 Timeline。
- 自定义字段：`ShotId`、`VoiceId`、`Notes`、`LocalizationKey`，不要把任意 JSON 全塞进字段。

推荐一条剧情节点最多包含 1–3 个高层演出意图。若命令字符串超过一行、参数超过约 5 个或包含复杂重叠时间轴，则引用 `ShotPreset` 或 Timeline，而不是继续堆字符串。

### 常见做法与最佳实践的区别

常见做法是直接在每句台词的 Sequence 中写 `Camera()`、`MoveTo()`、`AnimatorPlayWait()` 等底层命令；小项目可行，但资源改名、批量调节节奏和读档恢复会越来越困难。最佳实践是剧情节点引用高层语义，例如 `VNShot(confession_closeup)`，实际镜头距离、入场曲线和时长在预设资产中维护。

常见做法是用角色名查找 GameObject；最佳实践是稳定 ID + Catalog + 运行时注册表。常见做法是每条台词保存完整画面；最佳实践是保存“状态差量”，同时让系统能计算任意节点的完整快照用于预览和读档。常见做法是把所有东西做成 Timeline；最佳实践是普通台词走数据驱动命令，只有精确分镜走 Timeline，否则数百条 Timeline 会比对话数据库更难维护。

### 制作工具优先级

第一优先不是做完整节点编辑器，而是以下四个小工具：

1. `VN Catalog Window`：管理角色、表情、背景、镜头和效果 ID。
2. `Sequence Command Builder`：用下拉菜单生成合法命令，不手输 ID。
3. `VN Validator`：扫描缺图、无效 ID、不可达分支和重复变量。
4. `Preview From Entry`：从指定 Dialogue Entry 构建快照并播放该句演出。

等纵向切片稳定后，再考虑统一的“分镜表”编辑器、表格同步和波形/配音对齐。编辑器工具必须调用与运行时相同的命令与 Catalog，不能复制一套预览逻辑。

### 第一版明确不做

- 不从零重写分支图编辑器、Lua/条件系统或存档系统。
- 不同时引入 Ink、Yarn、Naninovel 等第二套叙事框架。
- 不立即安装 Cinemachine。
- 不先支持无限角色槽位、任意嵌套并行语法或复杂回滚。
- 不一次性封装全部 31 个 VNEffects；先接入背景、角色、表情、镜头、天气、转场与选项七条关键路径。

### 最小纵向切片验收脚本

制作一个约 60–90 秒样例：教室日景淡入；Yuki 从左侧进入 Center；表情由 neutral 切换为 smile；镜头缓慢推进并停顿 0.4 秒；天气从无切换为轻雨；另一角色从 Right 进入；出现两个有条件的选项；两个分支分别改变变量并进入不同背景；存档后重新加载应直接恢复背景、两名角色、表情、槽位、天气和镜头终态。这个样例通过后，架构才允许扩展到更多特效和批量内容生产。

## 14. VN 通用系统第一版实现记录（2026-07-13）

本轮在保留分支 `agent/vn-foundation` 上实现以下内容：

- `VNContentCatalog`：集中管理角色、表情、背景、镜头和天气预设，使用忽略大小写的稳定 ID 查询。
- `VNStageController`：背景 A/B 双缓冲交叉淡化、五个构图槽位和五实例角色池。
- `VNCharacterView`：立绘显示、表情切换、移动、隐藏；若配置 `VNEntranceAnimator` 则复用高级演出，否则自动降级为基础淡入/滑动。
- `VNDirector`：统一提供背景、角色、表情、移动、镜头、天气、通用特效和等待 API，并支持 `instant` 模式。
- `VNEffectDirector`：用 Inspector 的 UnityEvent binding 接入现有心跳、景深、震动等组件，不让剧情代码依赖具体实现。
- `VNStateSnapshot`：JSON 化保存和恢复背景、角色、表情、槽位、镜头、天气与持续特效。
- Dialogue System 命令：`VNBG`、`VNChar`、`VNFace`、`VNMove`、`VNHide`、`VNCamera`、`VNWeather`、`VNEffect`、`VNWait`；命令会等待 DOTween 完成，并在取消时清理。
- 编辑器工具：一键创建 VN Runtime Rig、自动补 Dialogue Manager、Catalog 校验、Sequence Command Builder。
- 默认资产：`Assets/VNSystem/Data/VNContentCatalog.asset`，预置基础镜头和五种天气 ID。
- 使用文档：`Assets/Scripts/VNSystem/README.md`。

验证结果：项目当时已在两个 Unity Editor 进程中打开，Unity BatchMode 因工程锁无法启动，未擅自关闭用户编辑器。改用 `Temp/CodexValidation` 中不进入版本控制的一次性项目，分别按 Unity 程序集边界编译运行时和 Editor 代码；两者最终均为 **0 warnings / 0 errors**。同时修正 Unity 6 已弃用的 `FindObjectOfType`、命令生成器区域小数格式，以及角色入场同帧查找问题。

当前边界：系统代码与默认配置已完成，但没有擅自修改用户正在编辑的场景，也没有虚构角色/背景资源映射或剧情。下一步在 Unity 中执行 `Tools > VN System > Create or Repair Runtime Rig`，为 Catalog 指定真实 Sprite，并制作最小纵向切片 Conversation。

版本控制结果：实现提交为 `1c2345d`（`Add VN authoring foundation`），已普通推送到新分支 `agent/vn-foundation` 并设置跟踪 `origin/agent/vn-foundation`。远端 `main` 仍保持在 `a323ac1`，基线标签未移动；没有删除、强推或改写任何分支。
