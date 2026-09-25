try
{
    ApplicationConfiguration.Initialize();
    Application.Run(new MainForm());
}
catch (Exception ex)
{
    var msg = $"{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}";
    try
    {
        MessageBox.Show(msg, "Prism.Net 错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    catch
    {
        File.WriteAllText("prism_error.log",
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{msg}");
    }
}
