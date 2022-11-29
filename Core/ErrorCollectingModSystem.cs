using System.Diagnostics;

namespace JadeFables.Core;

/// <summary>
///     A <see cref="ModSystem"/> which collects and keeps track of errors that occur during loading, with the option to print them upon joining a world.
/// </summary>
public abstract class ErrorCollectingModSystem : ModSystem
{
    /// <summary>
    ///     Whether to print collected errors upon entering a world. <br />
    ///     By default, true if a debugger is attached.
    /// </summary>
    public virtual bool PrintOnWorldEnter => Debugger.IsAttached;

    /// <summary>
    ///     Collected load errors.
    /// </summary>
    public virtual List<ISystemLoadError> LoadErrors { get; } = new();

    /// <summary>
    ///     Adds an error to the list of collected errors.
    /// </summary>
    /// <param name="error"></param>
    protected virtual void AddError(ISystemLoadError error) {
        Action<object> logAction = error.Severity switch
        {
            LoadErrorSeverity.Debug => Mod.Logger.Debug,
            LoadErrorSeverity.Info => Mod.Logger.Info,
            LoadErrorSeverity.Warn => Mod.Logger.Warn,
            LoadErrorSeverity.Error => Mod.Logger.Error,
            LoadErrorSeverity.Fatal => Mod.Logger.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(error), "Invalid error severity: " + (int) error.Severity)
        };

        logAction(error.AsLoggable());
        LoadErrors.Add(error);
    }
}

// Would like to use JetBrains.Annotations, but I'm sure the other devs would complain.
// ReSharper disable once UnusedType.Global
internal sealed class ErrorReportingPlayer : ModPlayer
{
    public override void OnEnterWorld(Player player) {
        base.OnEnterWorld(player);

        foreach (var system in Mod.GetContent<ErrorCollectingModSystem>()) {
            if (system.PrintOnWorldEnter) continue;
            if (system.LoadErrors.Count == 0) continue;

            Main.NewText("");
            Main.NewText($"{system.Name} - Errors: {system.LoadErrors.Count}", Colors.RarityRed);

            foreach (var error in system.LoadErrors) Main.NewText('[' + error.Severity + ']' + error.AsReportable(), Colors.RarityRed);
        }
    }
}

/// <summary>
///     Represents the various levels of collectible system load error severities.
/// </summary>
public enum LoadErrorSeverity
{
    /// <summary>
    /// </summary>
    Debug,

    /// <summary>
    /// </summary>
    Info,

    /// <summary>
    /// </summary>
    Warn,

    /// <summary>
    /// </summary>
    Error,

    /// <summary>
    /// </summary>
    Fatal
}

/// <summary>
///     Represents a load error collected by an <see cref="ErrorCollectingModSystem"/>.
/// </summary>
public interface ISystemLoadError
{
    /// <summary>
    ///     The severity of this error.
    /// </summary>
    LoadErrorSeverity Severity { get; }

    /// <summary>
    ///     The string representation of this load error in a format befitting logging.
    /// </summary>
    string AsLoggable();

    /// <summary>
    ///     The string representation of this load error in a format befitting reporting in-game.
    /// </summary>
    string AsReportable();
}

/// <summary>
///     Default implementation of <see cref="ISystemLoadError"/>.
/// </summary>
public abstract class SystemLoadError : ISystemLoadError
{
    public virtual LoadErrorSeverity Severity { get; init; } = LoadErrorSeverity.Error;

    string ISystemLoadError.AsLoggable() {
        return GetType().Name + ": " + AsLoggableImpl();
    }

    protected abstract string AsLoggableImpl();

    string ISystemLoadError.AsReportable() {
        return GetType().Name + ": " + AsReportableImpl();
    }

    protected abstract string AsReportableImpl();
}