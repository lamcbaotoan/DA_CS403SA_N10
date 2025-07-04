using System.Collections.Generic;

namespace Webebook
{
    // Các lớp này mô phỏng cấu trúc JSON để giao tiếp với API của Gemini
    public class GeminiRequest
    {
        public List<Content> contents { get; set; }
    }
    public class Content
    {
        public List<Part> parts { get; set; }
    }
    public class Part
    {
        public string text { get; set; }
    }

    public class GeminiResponse
    {
        public List<Candidate> candidates { get; set; }
        public Error error { get; set; }
    }
    public class Candidate
    {
        public Content content { get; set; }
    }
    public class Error
    {
        public string message { get; set; }
    }

    // Lớp để chứa phản hồi có cấu trúc, bao gồm cả nút bấm
    public class ChatResponse
    {
        public string Text { get; set; }
        public string ButtonText { get; set; }
        public string ButtonUrl { get; set; }
    }
}