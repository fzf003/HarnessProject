---
name: /opsx-new
category: Workflow
description: 创建变更脚手架 - 在 openspec/changes/ 下生成空目录
---

创建一个新的 OpenSpec 变更脚手架。这是一个轻量级命令，只创建目录结构，不自动生成任何规划文件。

**适用场景**：
- 需要从零定制需求文档
- 避免 AI 一次性生成过多内容导致偏差
- 希望完全手动控制变更内容

---

**Input**: 变更名称（kebab-case 格式，如 `add-user-auth`）

**Steps**

1. **获取变更名称**

   如果用户提供了名称，直接使用。如果没有提供，询问用户：
   > "请提供变更名称（kebab-case 格式，如 add-user-auth）："

2. **检查是否已存在**

   检查 `openspec/changes/<name>/` 是否已存在：
   - 如果存在：询问用户是否要覆盖或选择其他名称
   - 如果不存在：继续创建

3. **创建变更目录**

   ```bash
   openspec new change "<name>"
   ```

   这会创建：
   ```
   openspec/changes/<name>/
   └── .openspec.yaml    # 变更配置文件
   ```

4. **确认创建成功**

   显示创建结果：
   ```
   ## 变更脚手架已创建

   **名称**: <name>
   **位置**: openspec/changes/<name>/

   目录结构：
   openspec/changes/<name>/
   └── .openspec.yaml

   下一步：
   - 手动创建 proposal.md、design.md、tasks.md 等文件
   - 或使用 /opsx:propose "描述" 自动生成完整规划
   - 或使用 /opsx:continue 逐步生成工件
   ```

**Guardrails**
- 变更名称使用 kebab-case（短横线连接的小写字母）
- 不自动生成任何规划文件，只创建空目录
- 如果目录已存在，必须获得用户确认才能覆盖
