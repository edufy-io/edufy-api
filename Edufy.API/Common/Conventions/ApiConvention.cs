using Edufy.API.Common.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Edufy.API.Common.Conventions;

public class ApiConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            ApplyToController(controller);

            foreach (var action in controller.Actions)
            {
                ApplyToAction(action, controller);
            }
        }
    }

    private static void ApplyToController(ControllerModel controller)
    {
        ApplyStandardResponse(controller.Attributes, controller.Filters);
        ApplyAuthorizedEndpoint(controller.Attributes, controller.Filters);
    }

    private static void ApplyToAction(ActionModel action, ControllerModel controller)
    {
        // StandardResponse — ưu tiên action trước, nếu không có thì check controller
        var hasStandardResponse = HasAttribute<StandardResponseAttribute>(action.Attributes)
            || HasAttribute<StandardResponseAttribute>(controller.Attributes);

        if (hasStandardResponse)
            AddStandardResponseTypes(action.Filters);

        // AuthorizedEndpoint — chỉ check ở action level
        var authorizedAttr = action.Attributes
            .OfType<AuthorizedEndpointAttribute>()
            .FirstOrDefault();

        if (authorizedAttr is not null)
        {
            AddAuthorizeFilter(action.Filters, authorizedAttr);
            AddAuthorizedResponseTypes(action.Filters);
        }
    }

    #region StandardResponse

    private static void ApplyStandardResponse(
        IReadOnlyList<object> attributes,
        IList<IFilterMetadata> filters)
    {
        if (!HasAttribute<StandardResponseAttribute>(attributes))
            return;

        AddStandardResponseTypes(filters);
    }

    private static void AddStandardResponseTypes(IList<IFilterMetadata> filters)
    {
        filters.Add(new ProducesResponseTypeAttribute(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest));

        filters.Add(new ProducesResponseTypeAttribute(
            typeof(ProblemDetails),
            StatusCodes.Status500InternalServerError));
    }

    #endregion

    #region AuthorizedEndpoint

    private static void ApplyAuthorizedEndpoint(
        IReadOnlyList<object> attributes,
        IList<IFilterMetadata> filters)
    {
        var authorizedAttr = attributes
            .OfType<AuthorizedEndpointAttribute>()
            .FirstOrDefault();

        if (authorizedAttr is null)
            return;

        AddAuthorizeFilter(filters, authorizedAttr);
        AddAuthorizedResponseTypes(filters);
    }

    private static void AddAuthorizeFilter(
        IList<IFilterMetadata> filters,
        AuthorizedEndpointAttribute attribute)
    {
        if (filters.OfType<AuthorizeFilter>().Any())
            return;

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser();

        if (!string.IsNullOrEmpty(attribute.Role))
            policy.RequireRole(attribute.Role);

        if (!string.IsNullOrEmpty(attribute.Policy))
            policy.RequireAssertion(_ => true);

        filters.Add(new AuthorizeFilter(policy.Build()));
    }

    private static void AddAuthorizedResponseTypes(IList<IFilterMetadata> filters)
    {
        filters.Add(new ProducesResponseTypeAttribute(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized));

        filters.Add(new ProducesResponseTypeAttribute(
            typeof(ProblemDetails),
            StatusCodes.Status403Forbidden));
    }

    #endregion

    #region Helper

    private static bool HasAttribute<T>(IReadOnlyList<object> attributes)
        where T : Attribute
    {
        return attributes.OfType<T>().Any();
    }

    #endregion
}