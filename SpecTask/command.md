
---

## 🔄 典型工作流演示（以"添加深色模式"为例）

```bash
# 1️⃣ 启动变更（扩展模式）
/opsx:new add-dark-mode
# → 创建 openspec/changes/add-dark-mode/ 目录

# 2️⃣ 生成规划工件（二选一）
/opsx:ff                    # 一键生成 proposal/specs/design/tasks
# 或
/opsx:continue → 逐步确认每个工件

# 3️⃣ 开始实现
/opsx:apply
# AI 读取 tasks.md，逐个执行任务，自动打勾 [x]

# 4️⃣ 质量检查（可选但推荐）
/opsx:verify
# 输出：
# ✓ Completeness: 12/12 tasks done
# ⚠ Warning: 缺少"系统主题切换"的测试用例
# → 可补充任务后继续，或直接归档

# 5️⃣ 归档收尾
/opsx:archive
# → 合并 delta specs 到 openspec/specs/ui/spec.md
# → 变更移入 openspec/changes/archive/2026-04-18-add-dark-mode/