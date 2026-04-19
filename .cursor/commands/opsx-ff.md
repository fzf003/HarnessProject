---
name: /opsx-ff
category: Workflow
description: 一键生成全部工件 - 跳过逐步确认，直接生成完整规划
---

一键生成 OpenSpec 变更的全部工件。适合需求明确且结构成熟的场景，希望快速推进到编码阶段。

**生成内容**: proposal → specs → design → tasks（一次性全部生成）

---

**Input**: 可选，变更名称（如 `/opsx:ff add-auth`）。如果未提供，从对话上下文推断或提示选择。

**Steps**

1. **确定变更名称**

   - 如果提供了名称，直接使用
   - 从对话上下文推断
   - 如果模糊或不存在，运行 `openspec list --json` 并提示用户选择

2. **检查当前状态**

   ```bash
   openspec status --change "<name>" --json
   ```

   解析 JSON 了解：
   - `schemaName`: 使用的工作流模式
   - `artifacts`: 各工件当前状态
   - `applyRequires`: 实施前需要完成的工件列表

3. **获取构建顺序**

   根据依赖关系确定工件构建顺序。典型顺序：
   1. proposal.md（基础，无依赖）
   2. specs/（依赖 proposal）
   3. design.md（依赖 proposal/specs）
   4. tasks.md（依赖 design）

4. **循环生成所有待完成的工件**

   对于每个状态为 `pending` 的工件：

   a. **获取生成指令**
      ```bash
      openspec instructions <artifact-id> --change "<name>" --json
      ```

   b. **读取依赖文件**
      - 从 `dependencies` 中读取已完成的工件作为上下文

   c. **生成工件**
      - 使用 `template` 作为结构
      - 遵循 `instruction` 指导
      - 应用 `context` 和 `rules` 作为约束
      - 创建文件

   d. **更新状态**
      - 重新运行 `openspec status` 检查进度
      - 继续下一个工件

5. **显示完成摘要**

   ```
   ## 全部工件已生成

   **变更**: <name>
   **模式**: <schema-name>

   已生成文件：
   ✅ proposal.md    - 需求提案（目标、范围、验收标准）
   ✅ specs/         - 能力规范（需求与场景）
   ✅ design.md      - 设计方案（架构、接口、决策）
   ✅ tasks.md       - 实现任务（可执行的步骤清单）

   位置：openspec/changes/<name>/

   下一步：
   - 审查生成的规划文件
   - 运行 `/opsx:apply` 开始代码实现
   - 如需修改，直接编辑对应文件
   ```

**Output On Partial Exists**

如果部分工件已存在：

```
## 工件生成完成

**变更**: <name>

工件状态：
- [x] proposal.md    （已存在，跳过）
- [x] specs/         （已存在，跳过）
- [x] design.md      （新生成）
- [x] tasks.md       （新生成）

新生成：2 个工件
已存在：2 个工件（未覆盖）

准备就绪！运行 `/opsx:apply` 开始实现。
```

**Guardrails**
- 按依赖顺序生成，确保前置工件先生成
- 已存在的工件默认跳过（询问用户是否覆盖）
- 如果生成过程中遇到不明确的需求，暂停并询问用户
- 保持工件之间的一致性，后生成的工件引用前置工件内容
- 生成完成后提示用户审查，不要直接进入实现阶段
