using System;
using System.Collections.Generic;
using System.IO;

public static class EnvReader
{
    public static Dictionary<string, string> LoadEnv(string filePath)
    {
        var envVars = new Dictionary<string, string>();

        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split(new[] { '=' }, 2);
            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            envVars[key] = value;
        }

        return envVars;
    }
}
