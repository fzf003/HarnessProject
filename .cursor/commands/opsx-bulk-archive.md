---
name: /opsx-bulk-archive
category: Workflow
description: 批量归档 - 扫描所有标记完成的变更，一键执行归档与规范合并
---

批量归档所有已完成的 OpenSpec 变更。适合并行开发多条功能线后，统一收尾与资产沉淀的场景。

---

**Input**: 无参数，自动扫描所有可归档的变更。

**Steps**

1. **扫描所有变更**

   ```bash
   openspec list --json
   ```

   解析结果，识别：
   - 活跃的变更（不在 archive 中）
   - 每个变更的完成状态
   - 哪些变更有 delta specs 需要同步

2. **评估可归档的变更**

   对于每个活跃变更，检查：

   a. **工件完整性**
      - 运行 `openspec status --change "<name>" --json`
      - 检查所有 `applyRequires` 工件是否状态为 `done`

   b. **任务完成度**
      - 读取 tasks.md（如果存在）
      - 统计 `- [ ]` vs `- [x]` 任务

   c. **Delta Spec 状态**
      - 检查 `openspec/changes/<name>/specs/` 是否存在
      - 如存在，标记需要同步

3. **生成分类列表**

   将变更分为三类：

   ```
   ## 批量归档评估

   **可归档**（所有检查通过）：
   - [ ] add-auth          - 所有工件完成，所有任务完成
   - [ ] fix-login-bug     - 所有工件完成，所有任务完成

   **需同步后归档**（有 delta specs）：
   - [ ] update-api        - 有 2 个 delta specs 待同步

   **未完成**（无法归档）：
   - add-payment           - 3/5 任务未完成
   - refactor-db           - design.md 未生成
   ```

4. **提示用户选择**

   使用 **AskUserQuestion tool** 让用户选择：
   - 哪些"可归档"的变更要归档
   - 是否先同步"需同步后归档"的变更

5. **执行批量归档**

   对于用户确认的每个变更：

   a. **如需同步，先执行同步**
      - 调用 `opsx-sync` 技能同步 delta specs
      - 或提示用户运行 `/opsx:sync <name>`

   b. **执行归档**
      - 创建归档目录 `openspec/changes/archive/YYYY-MM-DD-<name>/`
      - 移动变更目录到归档位置

   c. **记录结果**
      - 记录成功/失败状态

6. **显示归档摘要**

   ```
   ## 批量归档完成

   **成功归档**（3 个）：
   ✅ add-auth          → archive/2024-01-15-add-auth/
   ✅ fix-login-bug     → archive/2024-01-15-fix-login-bug/
   ✅ update-api        → archive/2024-01-15-update-api/（已同步 specs）

   **跳过**（2 个）：
   ⏭️ add-payment       - 用户取消
   ⏭️ refactor-db       - 未完成，无法归档

   **归档位置**: openspec/changes/archive/
   ```

**Output On Nothing to Archive**

```
## 批量归档

没有找到可归档的变更。

所有活跃变更状态：
- add-payment     - 进行中（3/5 任务）
- refactor-db     - 规划中（缺少 design.md）

提示：使用 `/opsx:continue` 或 `/opsx:apply` 推进变更。
```

**Guardrails**
- 只归档用户明确选择的变更
- 未完成或部分完成的变更默认不显示为可选项
- 有 delta specs 的变更提示同步，但不强制
- 如果归档目录已存在，添加序号或询问用户
- 保留所有 `.openspec.yaml` 配置文件
- 归档后变更从活跃列表中移除
