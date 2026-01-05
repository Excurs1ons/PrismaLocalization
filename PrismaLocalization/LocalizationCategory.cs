namespace PrismaLocalization;

/// <summary>
/// Categories for localization entries.
/// Useful for organizing and filtering translations.
/// </summary>
public enum LocalizationCategory
{
    /// <summary>
    /// Uncategorized text.
    /// </summary>
    General = 0,

    /// <summary>
    /// Nouns - names of things, people, places, etc.
    /// </summary>
    Noun = 100,

    /// <summary>
    /// Verbs - action words.
    /// </summary>
    Verb = 200,

    /// <summary>
    /// Adjectives - descriptive words.
    /// </summary>
    Adjective = 300,

    /// <summary>
    /// Pronouns - words referring to people (he, she, they, etc.)
    /// </summary>
    Pronoun = 400,
    
    /// <summary>
    /// Numbers and quantities.
    /// </summary>
    Number = 500,

    /// <summary>
    /// Time and date expressions.
    /// </summary>
    Time = 600,

    /// <summary>
    /// UI labels and interface text.
    /// </summary>
    UI = 700,

    /// <summary>
    /// Error messages and warnings.
    /// </summary>
    Error = 800,

    /// <summary>
    /// Dialog and conversation text.
    /// </summary>
    Dialog = 900,

    /// <summary>
    /// Narrative text and descriptions.
    /// </summary>
    Narrative = 1000,

    /// <summary>
    /// Tutorial and help text.
    /// </summary>
    Tutorial = 1100,

    /// <summary>
    /// Menu and navigation text.
    /// </summary>
    Menu = 1200,

    /// <summary>
    /// Notification messages.
    /// </summary>
    Notification = 1300,
}
