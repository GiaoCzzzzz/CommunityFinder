using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityFinder.Models
{
    public class CategoryNode
    {
        public string Level1 { get; set; }  // 一级分类
        public string Level2 { get; set; }  // 二级分类
        public string Level3 { get; set; }  // 三级分类
        public string FullPath => $"{Level1}/{Level2}/{Level3}";
    }

    public class CategoryData
    {
        public List<CategoryNode> Categories { get; set; } = new();
    }
}
