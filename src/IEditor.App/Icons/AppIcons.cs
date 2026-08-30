using Avalonia.Media;

namespace IEditor.App.Icons;

public static class AppIcons
{
    public static readonly StreamGeometry LoadImage = StreamGeometry.Parse("M3 5.5A2.5 2.5 0 0 1 5.5 3h13A2.5 2.5 0 0 1 21 5.5v13A2.5 2.5 0 0 1 18.5 21h-13A2.5 2.5 0 0 1 3 18.5z M8.2 10.8a1.7 1.7 0 1 0 0-3.4a1.7 1.7 0 0 0 0 3.4z M5.5 17.3 9.4 13.1a1.5 1.5 0 0 1 2.2 0l4.1 4.2 M13.2 14.9l1.6-1.7a1.5 1.5 0 0 1 2.2 0l2.5 2.6");

    public static readonly StreamGeometry Crop = StreamGeometry.Parse("M6.5 2.5v13a2 2 0 0 0 2 2h13 M17.5 21.5v-13a2 2 0 0 0-2-2h-13");

    public static readonly StreamGeometry PhotoStudio = StreamGeometry.Parse("M3 5.5A2.5 2.5 0 0 1 5.5 3h13A2.5 2.5 0 0 1 21 5.5v13A2.5 2.5 0 0 1 18.5 21h-13A2.5 2.5 0 0 1 3 18.5z M8.2 10.8a1.7 1.7 0 1 0 0-3.4a1.7 1.7 0 0 0 0 3.4z M5.5 17.3 9.4 13.1a1.5 1.5 0 0 1 2.2 0l4.1 4.2 M13.2 14.9l1.6-1.7a1.5 1.5 0 0 1 2.2 0l2.5 2.6");

    public static readonly StreamGeometry BatchProcessing = StreamGeometry.Parse("M8 6h10a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2z M6 10H4a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v2 M10 10v8");

    public static readonly StreamGeometry SmartCutout = StreamGeometry.Parse("M12 3.5 13.5 8l4.5 1.5-4.5 1.5L12 15.5l-1.5-4.5L6 9.5 10.5 8z M18.5 13.8l.7 2.1 2.1.7-2.1.7-.7 2.1-.7-2.1-2.1-.7 2.1-.7z");

    public static readonly StreamGeometry Toolbox = StreamGeometry.Parse("M4 7a2 2 0 0 1 2-2h4l1.8 2H18a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2z M8 11h8 M8 15h8");

    public static readonly StreamGeometry Settings = StreamGeometry.Parse("M12 3.5a8.5 8.5 0 1 0 0 17a8.5 8.5 0 1 0 0-17z M12 8v4.2 M12 15.8h.01 M7.7 12a4.3 4.3 0 0 1 8.6 0");

    public static readonly StreamGeometry Help = StreamGeometry.Parse("M12 3.5a8.5 8.5 0 1 0 0 17a8.5 8.5 0 1 0 0-17z M9.6 9.2a2.4 2.4 0 1 1 4.8.6c0 1.7-2.4 2-2.4 3.4 M12 16.7h.01");

    public static readonly StreamGeometry Theme = StreamGeometry.Parse("M20 13.6A8.1 8.1 0 0 1 10.4 4 6.4 6.4 0 1 0 20 13.6z");

    public static readonly StreamGeometry Export = StreamGeometry.Parse("M12 4v11 M7.5 11.5 12 16l4.5-4.5 M4.5 19.5h15");

    public static readonly StreamGeometry Add = StreamGeometry.Parse("M12 5v14 M5 12h14");

    public static readonly StreamGeometry Folder = StreamGeometry.Parse("M3.5 7a2 2 0 0 1 2-2h4.1l1.9 2.2h6.9a2 2 0 0 1 2 2V17a2 2 0 0 1-2 2h-13a2 2 0 0 1-2-2z");

    public static readonly StreamGeometry Refresh = StreamGeometry.Parse("M3.5 12a8.5 8.5 0 0 1 14.8-5.7L20 8.5M20.5 3.5v5h-5 M20.5 12a8.5 8.5 0 0 1-14.8 5.7L4 15.5M3.5 20.5v-5h5");

    public static readonly StreamGeometry Preview = StreamGeometry.Parse("M3.5 5.5h17v13h-17z M9 10.5a2 2 0 1 0 0.01 0 M5.8 16.2c0-1.5 1.8-2.6 3.7-2.6s3.7 1.1 3.7 2.6 M15.8 10.2h3 M15.8 13.8h2.4");

    public static readonly StreamGeometry RotateLeft = StreamGeometry.Parse("M4 12a8 8 0 1 0 2.6-5.9L4 8.7 M4 4.5v4.2h4.2");

    public static readonly StreamGeometry RotateRight = StreamGeometry.Parse("M20 12a8 8 0 1 1-2.6-5.9L20 8.7 M20 4.5v4.2h-4.2");

    public static readonly StreamGeometry FlipHorizontal = StreamGeometry.Parse("M12 3.5v17 M8 7.5 4.2 12 8 16.5z M16 7.5 19.8 12 16 16.5z");

    public static readonly StreamGeometry Guides = StreamGeometry.Parse("M4 4h16v16H4z M8.5 4v16 M15.5 4v16 M4 8.5h16 M4 15.5h16");

    public static readonly StreamGeometry Compare = StreamGeometry.Parse("M4 5h16v14H4z M12 5v14");

    public static readonly StreamGeometry Fit = StreamGeometry.Parse("M6 6h4V4 M14 4h4v4 M18 18h-4v2 M10 20H6v-4");

    public static readonly StreamGeometry Remove = StreamGeometry.Parse("M4 7h16 M9.5 7V5.2A1.2 1.2 0 0 1 10.7 4h2.6A1.2 1.2 0 0 1 14.5 5.2V7 M6.5 7l.8 12a2 2 0 0 0 2 1.9h5.4a2 2 0 0 0 2-1.9l.8-12");

    public static readonly StreamGeometry Zip = StreamGeometry.Parse("M6 3.5h8l4 4v13h-12z M10 3.5v4h4 M10 10h4 M10 13h4");

    public static readonly StreamGeometry Check = StreamGeometry.Parse("M5 12.5 9.2 16.7 19 7");

    public static readonly StreamGeometry Menu = StreamGeometry.Parse("M5 7h14 M5 12h14 M5 17h14");

    public static readonly StreamGeometry Zap = StreamGeometry.Parse("M13 3 5 13.5h5L11 21l8-10.5h-5Z");

    public static readonly StreamGeometry Palette = StreamGeometry.Parse("M12 3.5a8.5 8.5 0 1 0 0 17c1.4 0 2-.8 2-1.7 0-.8-.6-1.3-.6-2 0-.9.7-1.6 1.8-1.6h1.6c2 0 3.7-1.6 3.7-3.6 0-4.9-3.8-8.1-8.5-8.1z");
}
