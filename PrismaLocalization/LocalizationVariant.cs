namespace PrismaLocalization;

/// <summary>
/// Represents grammatical variants for localization entries.
/// Used for handling inflections based on grammatical context.
/// </summary>
[Flags]
public enum LocalizationVariant
{
    /// <summary>
    /// No specific variant - the base form.
    /// </summary>
    None = 0,

    // ========== Number Variants (for Nouns) ==========
    /// <summary>
    /// Singular form (e.g., "item", "apple")
    /// </summary>
    Singular = 1 << 0,

    /// <summary>
    /// Plural form (e.g., "items", "apples")
    /// </summary>
    Plural = 1 << 1,

    /// <summary>
    /// Dual form (for languages that have dual grammatical number)
    /// </summary>
    Dual = 1 << 2,

    /// <summary>
    /// Paucal form (for a few items)
    /// </summary>
    Paucal = 1 << 3,

    // ========== Case Variants (for Nouns, Pronouns) ==========
    /// <summary>
    /// Nominative case (subject form) - e.g., "I", "he", "they"
    /// </summary>
    Nominative = 1 << 4,

    /// <summary>
    /// Accusative case (direct object form) - e.g., "me", "him", "them"
    /// </summary>
    Accusative = 1 << 5,

    /// <summary>
    /// Genitive case (possessive form without noun) - e.g., "mine", "his", "theirs"
    /// </summary>
    Genitive = 1 << 6,

    /// <summary>
    /// Dative case (indirect object form)
    /// </summary>
    Dative = 1 << 7,

    /// <summary>
    /// Possessive determiner (before noun) - e.g., "my", "his", "their"
    /// </summary>
    PossessiveDeterminer = 1 << 8,

    /// <summary>
    /// Possessive pronoun (standalone) - e.g., "mine", "his", "theirs"
    /// </summary>
    PossessivePronoun = 1 << 9,

    // ========== Gender Variants (for Pronouns, Adjectives) ==========
    /// <summary>
    /// Masculine gender
    /// </summary>
    Masculine = 1 << 10,

    /// <summary>
    /// Feminine gender
    /// </summary>
    Feminine = 1 << 11,

    /// <summary>
    /// Neuter gender
    /// </summary>
    Neuter = 1 << 12,

    // ========== Person Variants (for Pronouns, Verbs) ==========
    /// <summary>
    /// First person (I, we)
    /// </summary>
    FirstPerson = 1 << 13,

    /// <summary>
    /// Second person (you)
    /// </summary>
    SecondPerson = 1 << 14,

    /// <summary>
    /// Third person (he, she, it, they)
    /// </summary>
    ThirdPerson = 1 << 15,

    // ========== Tense Variants (for Verbs) ==========
    /// <summary>
    /// Present tense
    /// </summary>
    Present = 1 << 16,

    /// <summary>
    /// Past tense
    /// </summary>
    Past = 1 << 17,

    /// <summary>
    /// Future tense
    /// </summary>
    Future = 1 << 18,

    /// <summary>
    /// Conditional mood
    /// </summary>
    Conditional = 1 << 19,

    // ========== Common Combinations ==========
    /// <summary>
    /// Default noun form (singular nominative)
    /// </summary>
    DefaultNoun = Singular | Nominative,

    /// <summary>
    /// Default pronoun form (singular first person nominative)
    /// </summary>
    DefaultPronoun = FirstPerson | Singular | Nominative,

    /// <summary>
    /// Subject form (I, he, she, we, they)
    /// </summary>
    Subject = Nominative,

    /// <summary>
    /// Object form (me, him, her, us, them)
    /// </summary>
    Object = Accusative,
}

/// <summary>
/// Extension methods for LocalizationVariant.
/// </summary>
public static class LocalizationVariantExtensions
{
    /// <summary>
    /// Checks if the variant contains any of the specified flags.
    /// </summary>
    public static bool HasAny(this LocalizationVariant value, LocalizationVariant flags)
    {
        return (value & flags) != 0;
    }

    /// <summary>
    /// Gets a human-readable description of the variant.
    /// </summary>
    public static string Describe(this LocalizationVariant variant)
    {
        var parts = new List<string>();

        if (variant.HasAny(LocalizationVariant.Singular)) parts.Add("Singular");
        if (variant.HasAny(LocalizationVariant.Plural)) parts.Add("Plural");
        if (variant.HasAny(LocalizationVariant.Dual)) parts.Add("Dual");
        if (variant.HasAny(LocalizationVariant.Paucal)) parts.Add("Paucal");
        if (variant.HasAny(LocalizationVariant.Nominative)) parts.Add("Nominative");
        if (variant.HasAny(LocalizationVariant.Accusative)) parts.Add("Accusative");
        if (variant.HasAny(LocalizationVariant.Genitive)) parts.Add("Genitive");
        if (variant.HasAny(LocalizationVariant.Dative)) parts.Add("Dative");
        if (variant.HasAny(LocalizationVariant.PossessiveDeterminer)) parts.Add("PossessiveDet");
        if (variant.HasAny(LocalizationVariant.PossessivePronoun)) parts.Add("Possessive");
        if (variant.HasAny(LocalizationVariant.Masculine)) parts.Add("Masculine");
        if (variant.HasAny(LocalizationVariant.Feminine)) parts.Add("Feminine");
        if (variant.HasAny(LocalizationVariant.Neuter)) parts.Add("Neuter");
        if (variant.HasAny(LocalizationVariant.FirstPerson)) parts.Add("1stPerson");
        if (variant.HasAny(LocalizationVariant.SecondPerson)) parts.Add("2ndPerson");
        if (variant.HasAny(LocalizationVariant.ThirdPerson)) parts.Add("3rdPerson");
        if (variant.HasAny(LocalizationVariant.Present)) parts.Add("Present");
        if (variant.HasAny(LocalizationVariant.Past)) parts.Add("Past");
        if (variant.HasAny(LocalizationVariant.Future)) parts.Add("Future");
        if (variant.HasAny(LocalizationVariant.Conditional)) parts.Add("Conditional");

        return parts.Count == 0 ? "None" : string.Join(" | ", parts);
    }

    /// <summary>
    /// Creates a variant key suffix for storing variants.
    /// </summary>
    public static string ToSuffix(this LocalizationVariant variant)
    {
        if (variant == LocalizationVariant.None) return "";

        var parts = new List<string>();

        // Number
        if (variant.HasAny(LocalizationVariant.Singular)) parts.Add("sg");
        else if (variant.HasAny(LocalizationVariant.Plural)) parts.Add("pl");
        else if (variant.HasAny(LocalizationVariant.Dual)) parts.Add("du");
        else if (variant.HasAny(LocalizationVariant.Paucal)) parts.Add("pa");

        // Case
        if (variant.HasAny(LocalizationVariant.Nominative)) parts.Add("nom");
        if (variant.HasAny(LocalizationVariant.Accusative)) parts.Add("acc");
        if (variant.HasAny(LocalizationVariant.Genitive)) parts.Add("gen");
        if (variant.HasAny(LocalizationVariant.Dative)) parts.Add("dat");
        if (variant.HasAny(LocalizationVariant.PossessiveDeterminer)) parts.Add("posdet");
        if (variant.HasAny(LocalizationVariant.PossessivePronoun)) parts.Add("pospro");

        // Gender
        if (variant.HasAny(LocalizationVariant.Masculine)) parts.Add("m");
        if (variant.HasAny(LocalizationVariant.Feminine)) parts.Add("f");
        if (variant.HasAny(LocalizationVariant.Neuter)) parts.Add("n");

        // Person
        if (variant.HasAny(LocalizationVariant.FirstPerson)) parts.Add("p1");
        if (variant.HasAny(LocalizationVariant.SecondPerson)) parts.Add("p2");
        if (variant.HasAny(LocalizationVariant.ThirdPerson)) parts.Add("p3");

        // Tense
        if (variant.HasAny(LocalizationVariant.Present)) parts.Add("pres");
        if (variant.HasAny(LocalizationVariant.Past)) parts.Add("past");
        if (variant.HasAny(LocalizationVariant.Future)) parts.Add("fut");
        if (variant.HasAny(LocalizationVariant.Conditional)) parts.Add("cond");

        return parts.Count > 0 ? "_" + string.Join("_", parts) : "";
    }
}
