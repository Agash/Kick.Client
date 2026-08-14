using Kick.Client.Authentication;

namespace Kick.Client.Tests.Authentication;

[TestClass]
public sealed class KickPkceFlowHelperTests
{
    // RFC 7636 appendix B worked example.
    private const string RfcVerifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
    private const string RfcChallenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

    [TestMethod]
    public void DeriveCodeChallenge_MatchesRfc7636Vector()
        => Assert.AreEqual(RfcChallenge, KickPkceFlowHelper.DeriveCodeChallenge(RfcVerifier));

    [TestMethod]
    public void DeriveCodeChallenge_IsDeterministic()
    {
        string first = KickPkceFlowHelper.DeriveCodeChallenge(RfcVerifier);
        string second = KickPkceFlowHelper.DeriveCodeChallenge(RfcVerifier);

        Assert.AreEqual(first, second);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void DeriveCodeChallenge_WhenVerifierIsBlank_Throws(string? verifier)
        => Assert.Throws<ArgumentException>(() => KickPkceFlowHelper.DeriveCodeChallenge(verifier!));

    [TestMethod]
    public void GenerateCodeVerifier_IsBase64UrlWithoutPadding()
    {
        string verifier = KickPkceFlowHelper.GenerateCodeVerifier();

        Assert.DoesNotContain("=", verifier);
        Assert.DoesNotContain("+", verifier);
        Assert.DoesNotContain("/", verifier);
    }

    [TestMethod]
    public void GenerateCodeVerifier_RespectsRfc7636LengthBounds()
    {
        string verifier = KickPkceFlowHelper.GenerateCodeVerifier();

        // RFC 7636 section 4.1 constrains the verifier to 43..128 characters.
        Assert.IsInRange(43, 128, verifier.Length);
    }

    [TestMethod]
    public void GenerateCodeVerifier_ProducesDistinctValues()
    {
        string[] verifiers =
            [.. Enumerable.Range(0, 32).Select(_ => KickPkceFlowHelper.GenerateCodeVerifier())];

        Assert.AreAllDistinct(verifiers);
    }

    private static KickOAuthOptions Options => new()
    {
        ClientId = "client-123",
        RedirectUri = "https://example.test/callback",
        Scopes = "user:read channel:read",
    };

    [TestMethod]
    public void BuildAuthorizationUrl_IncludesTheS256PkceParameters()
    {
        string url = KickPkceFlowHelper.BuildAuthorizationUrl(
            "https://id.kick.test", Options, RfcChallenge, "state-xyz");

        Assert.Contains($"code_challenge={RfcChallenge}", url);
        Assert.Contains("code_challenge_method=S256", url);
        Assert.Contains("response_type=code", url);
        Assert.Contains("client_id=client-123", url);
        Assert.Contains("state=state-xyz", url);
    }

    [TestMethod]
    public void BuildAuthorizationUrl_PercentEncodesRedirectAndScopes()
    {
        string url = KickPkceFlowHelper.BuildAuthorizationUrl(
            "https://id.kick.test", Options, RfcChallenge, "state-xyz");

        Assert.Contains("redirect_uri=https%3A%2F%2Fexample.test%2Fcallback", url);
        Assert.Contains("scope=user%3Aread%20channel%3Aread", url);
    }

    [TestMethod]
    public void BuildAuthorizationUrl_TrimsTrailingSlashFromBaseUrl()
    {
        string url = KickPkceFlowHelper.BuildAuthorizationUrl(
            "https://id.kick.test/", Options, RfcChallenge, "state-xyz");

        Assert.StartsWith("https://id.kick.test/oauth/authorize?", url);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void BuildAuthorizationUrl_WhenBaseUrlIsBlank_Throws(string? baseUrl)
        => Assert.Throws<ArgumentException>(
            () => KickPkceFlowHelper.BuildAuthorizationUrl(baseUrl!, Options, RfcChallenge, "state"));

    [TestMethod]
    public void BuildAuthorizationUrl_WhenOptionsAreNull_Throws()
        => Assert.Throws<ArgumentNullException>(
            () => KickPkceFlowHelper.BuildAuthorizationUrl("https://id.kick.test", null!, RfcChallenge, "state"));
}
