# Tasks: <变更名称>

> 📌 关联工件：[proposal](./proposal.md) | [design](./design.md) | [specs](./specs/)

## 🎯 完成标准
- [ ] 所有任务打勾 ✅
- [ ] 代码通过本地测试
- [ ] `/opsx:verify` 无严重问题

---

## 1. 基础设施层
> 对应 design.md 的 "技术选型" 部分

- [ ] 1.1 创建核心 Context/Service 文件
  ```ts
  // 预期输出：src/context/ThemeContext.tsx
  // 关键逻辑：light/dark 状态 + localStorage 同步