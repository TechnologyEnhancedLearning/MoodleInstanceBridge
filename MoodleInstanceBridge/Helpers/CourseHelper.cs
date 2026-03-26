namespace MoodleInstanceBridge.Helpers
{
    public static class CourseHelper
    {
        private static readonly string _coursePath = "course/view.php";

        public static string GetCourseUrl(string apiBaseUrl, int? courseId)
        {
            return $"{apiBaseUrl}{_coursePath}?id={courseId}";
        }
    }
}
