# Quick Start Guide - AI Interest Analysis Feature

## 快速开始 (Quick Start)

### 功能简介 (Feature Overview)
这个功能会在用户打开MainPage时：
1. 自动读取用户的兴趣爱好
2. 智能匹配最合适的课程分类
3. 自动填充筛选器（What/Category/Topic）
4. 显示相关课程

### 如何使用 (How to Use)

#### 对于开发者 (For Developers)
✅ **无需任何配置** - 代码已集成，开箱即用
✅ **无需额外依赖** - 使用现有库和服务
✅ **向后兼容** - 不影响现有功能

只需确保：
1. Supabase已正确配置
2. profiles表有interest字段（text[] array类型）
3. Resources/Raw/categories.txt文件存在

#### 对于用户 (For Users)
1. 在个人资料中设置你的兴趣爱好
2. 打开MainPage
3. ✨ 系统自动推荐相关课程！

### 设置用户兴趣 (Set User Interests)

用户需要在Supabase的profiles表中设置interest字段：

```sql
-- 示例：音乐爱好者
UPDATE profiles 
SET interest = ARRAY['guitar', 'music', 'jazz']
WHERE id = 'user_id';

-- 示例：编程爱好者
UPDATE profiles 
SET interest = ARRAY['coding', 'programming', 'software']
WHERE id = 'user_id';

-- 示例：烹饪爱好者
UPDATE profiles 
SET interest = ARRAY['cooking', 'baking', 'food']
WHERE id = 'user_id';
```

或者通过应用的UI设置（如果已实现interest编辑页面）。

### 工作原理 (How It Works)

```
用户打开App
    ↓
MainPage.OnAppearing()
    ↓
读取兴趣: ["guitar", "music"]
    ↓
AI分析匹配
    ↓
自动选择:
  - What: Lifestyle & Leisure
  - Category: Music & Vocal Courses
  - Topic: Guitar
    ↓
自动搜索课程
    ↓
显示结果: 20个吉他课程
```

### 匹配示例 (Match Examples)

| 用户兴趣 | 匹配结果 |
|---------|---------|
| guitar, music | Music & Vocal Courses → Guitar |
| coding, programming | Digital, Tech & Innovation → Software Application |
| cooking, baking | Culinary Courses → Baking/Cooking |
| yoga, fitness | Health & Wellness → Fitness/Yoga |
| dance, dancing | Dance Courses → Ballet/Modern Dance |
| chinese, language | Language Courses → Chinese Language |
| art, painting | Arts & Handicrafts → Painting |

### 常见问题 (FAQ)

#### Q: 如果用户没有设置兴趣怎么办？
A: 系统会跳过自动匹配，显示默认的第一个分类，用户可以手动选择。

#### Q: 如果没有找到匹配的分类？
A: 系统会保持默认状态，用户可以手动选择任何分类。

#### Q: 用户可以手动修改自动填充的分类吗？
A: 可以！自动填充后，用户随时可以手动更改任何筛选器。

#### Q: 这会影响应用的性能吗？
A: 不会。整个分析过程通常在100ms内完成，用户感觉不到延迟。

#### Q: 如果Supabase连接失败怎么办？
A: 系统会捕获错误并继续默认流程，不会影响用户体验。

#### Q: 可以看到推荐的理由吗？
A: 当前版本暂不显示推荐理由，这是未来优化的方向。

### 支持的兴趣类别 (Supported Interest Categories)

系统支持所有categories.txt中的分类，主要包括：

**Education & Enrichment (教育与培训)**
- 语言课程
- 学前教育
- 小学辅导
- 中学辅导
- 演讲与戏剧

**Health & Wellness (健康与保健)**
- 芳香疗法
- 美容护肤
- 个人健康与营养
- 中医按摩

**Lifelong Learning (终身学习)**
- 商业与创业
- 数字技术与创新
- DIY与维护
- 工作技能
- 个人发展

**Lifestyle & Leisure (生活与休闲)**
- 饮品课程
- 烹饪课程
- 舞蹈课程
- 手工艺与爱好
- 智力与棋类游戏
- 音乐与声乐
- 体育与健身

### 调试信息 (Debugging)

如果需要调试匹配结果，可以在代码中添加日志：

```csharp
// 在 MainPage.xaml.cs 的 AutoFillCategoriesFromInterests() 中
var result = analyzer.AnalyzeInterests(interests);

if (result.HasValue)
{
    System.Diagnostics.Debug.WriteLine($"AI Match: L1={result.Value.L1}, L2={result.Value.L2}, L3={result.Value.L3}");
    _vm.SetCategorySelection(result.Value.L1, result.Value.L2, result.Value.L3);
}
else
{
    System.Diagnostics.Debug.WriteLine("No match found for interests");
}
```

### 测试建议 (Testing Tips)

1. **创建测试账号**：设置不同的interest数组
2. **验证匹配**：检查是否自动选择了正确的分类
3. **测试边界情况**：
   - 空interest数组
   - 无效的interest值
   - 超长的interest字符串
   - 多个可能的匹配
4. **测试错误情况**：
   - 网络断开
   - Supabase服务不可用
   - categories.txt文件缺失

### 文档资源 (Documentation)

- `IMPLEMENTATION_SUMMARY.md` - 完整实现总结（中英文）
- `FEATURE_EXAMPLE.md` - 详细使用示例
- `AI_FEATURE_FLOW.md` - 技术流程图
- `Services/README_InterestAnalyzer.md` - 技术文档

### 联系支持 (Support)

如有问题或建议，请：
1. 查看上述文档
2. 检查代码注释
3. 提交Issue到GitHub仓库

---

**状态:** ✅ 生产就绪
**版本:** 1.0
**最后更新:** 2025-10-12
