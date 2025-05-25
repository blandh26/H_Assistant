using H_Assistant.Framework;

namespace H_Assistant.DocUtils
{
    /// <summary>
    /// dll库全局参数设置
    /// </summary>
    public static class AppConst
    {
        /// <summary>
        /// 修订日志
        /// </summary>
        public static string LOG_CHAPTER_NAME = LanguageHepler.GetLanguage("AppConstRevisionLog");

        /// <summary>
        /// 数据库表目录
        /// </summary>
        public static string TABLE_CHAPTER_NAME = LanguageHepler.GetLanguage("AppConstDirectory");

        /// <summary>
        /// 数据库表结构
        /// </summary>
        public static string TABLE_STRUCTURE_CHAPTER_NAME = LanguageHepler.GetLanguage("AppConstTable");
    }
}
