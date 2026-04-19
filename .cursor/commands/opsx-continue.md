---
name: /opsx-continue
category: Workflow
description: 逐步生成工件 - 按依赖顺序依次生成下一个文件
---

按依赖顺序逐步生成 OpenSpec 变更工件。适合边想边写、分步审查的场景，或需要多人协同确认的情况。

**生成顺序**: proposal → specs → design → tasks

---

**Input**: 可选，变更名称（如 `/opsx:continue add-auth`）。如果未提供，从对话上下文推断或提示选择。

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
   - `artifacts`: 各工件状态（pending/done）
   - 依赖关系

3. **找到下一个待生成的工件**

   根据依赖关系，找到第一个状态为 `pending` 且依赖已满足的工件。

   典型顺序：
   1. proposal.md（无依赖）
   2. specs/（依赖 proposal）
   3. design.md（依赖 proposal/specs）
   4. tasks.md（依赖 design）

4. **获取生成指令**

   ```bash
   openspec instructions <artifact-id> --change "<name>" --json
   ```

   返回包含：
   - `context`: 项目背景（约束条件，不包含在输出中）
   - `rules`: 工件特定规则（约束条件，不包含在输出中）
   - `template`: 输出文件的结构模板
   - `instruction`: 针对此工件类型的具体指导
   - `outputPath`: 文件输出路径
   - `dependencies`: 需要读取的已完成工件

5. **读取依赖文件**

   读取 `dependencies` 中列出的已完成工件，作为上下文。

6. **生成工件**

   - 使用 `template` 作为结构
   - 遵循 `instruction` 指导
   - 应用 `context` 和 `rules` 作为约束（但不复制到文件中）
   - 创建文件

7. **显示进度**

   ```
   ## 工件生成进度

   **变更**: <name>
   **刚完成**: <artifact-id>
   **位置**: <outputPath>

   总体进度：
   - [x] proposal
   - [x] specs
   - [ ] design    ← 下一个
   - [ ] tasks

   下一步：
   - 继续运行 /opsx:continue 生成下一个工件
   - 或运行 /opsx:ff 一键生成剩余所有工件
   - 或运行 /opsx:apply 开始实现（如果 tasks 已完成）
   ```

**Output On All Complete**

```
## 所有工件已生成

**变更**: <name>
**状态**: 所有工件完成

已生成：
- [x] proposal.md
- [x] specs/
- [x] design.md
- [x] tasks.md

准备就绪！运行 `/opsx:apply` 开始实现。
```

**Guardrails**
- 严格按依赖顺序生成，不跳过前置工件
- 每次只生成一个工件，让用户有机会审查
- 如果工件已存在，询问是否覆盖
- 始终读取依赖文件以保持一致性
