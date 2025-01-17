using HotChocolate.Execution.Processing;
using HotChocolate.Types;

namespace HotChocolate.Data.Projections;

public class SelectionVisitor<TContext>
    : SelectionVisitor
    where TContext : ISelectionVisitorContext
{
    protected virtual ISelectionVisitorAction Visit(
        IOutputField field,
        TContext context)
    {
        var localContext = OnBeforeEnter(field, context);
        var result = Enter(field, localContext);
        localContext = OnAfterEnter(field, localContext, result);

        if (result.Kind == SelectionVisitorActionKind.Continue)
        {
            if (VisitChildren(field, context).Kind == SelectionVisitorActionKind.Break)
            {
                return Break;
            }
        }

        if (result.Kind == SelectionVisitorActionKind.Continue ||
            result.Kind == SelectionVisitorActionKind.SkipAndLeave)
        {
            localContext = OnBeforeLeave(field, localContext);
            result = Leave(field, localContext);
            OnAfterLeave(field, localContext, result);
        }

        return result;
    }

    protected virtual TContext OnBeforeLeave(
        IOutputField field,
        TContext localContext) =>
        localContext;

    protected virtual TContext OnAfterLeave(
        IOutputField field,
        TContext localContext,
        ISelectionVisitorAction result) =>
        localContext;

    protected virtual TContext OnAfterEnter(
        IOutputField field,
        TContext localContext,
        ISelectionVisitorAction result) =>
        localContext;

    protected virtual TContext OnBeforeEnter(
        IOutputField field,
        TContext context) =>
        context;

    protected virtual ISelectionVisitorAction Visit(
        ISelection selection,
        TContext context)
    {
        var localContext = OnBeforeEnter(selection, context);
        var result = Enter(selection, localContext);
        localContext = OnAfterEnter(selection, localContext, result);

        if (result.Kind == SelectionVisitorActionKind.Continue)
        {
            if (VisitChildren(selection, context).Kind == SelectionVisitorActionKind.Break)
            {
                return Break;
            }
        }

        if (result.Kind == SelectionVisitorActionKind.Continue ||
            result.Kind == SelectionVisitorActionKind.SkipAndLeave)
        {
            localContext = OnBeforeLeave(selection, localContext);
            result = Leave(selection, localContext);
            OnAfterLeave(selection, localContext, result);
        }

        return result;
    }

    protected virtual TContext OnBeforeLeave(
        ISelection selection,
        TContext localContext) =>
        localContext;

    protected virtual TContext OnAfterLeave(
        ISelection selection,
        TContext localContext,
        ISelectionVisitorAction result) =>
        localContext;

    protected virtual TContext OnAfterEnter(
        ISelection selection,
        TContext localContext,
        ISelectionVisitorAction result) =>
        localContext;

    protected virtual TContext OnBeforeEnter(
        ISelection selection,
        TContext context) =>
        context;

    protected virtual ISelectionVisitorAction VisitChildren(IOutputField field, TContext context)
    {
        var type = field.Type;
        var selection = context.Selection.Peek();

        var namedType = type.NamedType();
        if (namedType.IsAbstractType())
        {
            var possibleTypes = context.Context.Schema.GetPossibleTypes(field.Type.NamedType());

            foreach (var possibleType in possibleTypes)
            {
                var result = VisitObjectType(field, possibleType, selection, context);

                if (result != Continue)
                {
                    return result;
                }
            }
        }
        else if (namedType is ObjectType a)
        {
            return VisitObjectType(field, a, selection, context);
        }

        return DefaultAction;
    }

    protected virtual ISelectionVisitorAction VisitObjectType(
        IOutputField field,
        ObjectType objectType,
        ISelection selection,
        TContext context)
    {
        context.ResolvedType.Push(field.Type.NamedType().IsAbstractType() ? objectType : null);

        try
        {
            var selections = context.Context.GetSelections(objectType, selection, true);

            for (var i = 0; i < selections.Count; i++)
            {
                var result = Visit((ISelection)selections[i], context);
                if (result.Kind is SelectionVisitorActionKind.Break)
                {
                    return Break;
                }
            }
        }
        finally
        {
            context.ResolvedType.Pop();
        }

        return DefaultAction;
    }

    protected virtual ISelectionVisitorAction VisitChildren(
        ISelection selection,
        TContext context)
    {
        var field = selection.Field;
        return Visit(field, context);
    }

    protected virtual ISelectionVisitorAction Enter(
        IOutputField field,
        TContext context) =>
        DefaultAction;

    protected virtual ISelectionVisitorAction Leave(
        IOutputField field,
        TContext context) =>
        DefaultAction;

    protected virtual ISelectionVisitorAction Enter(
        ISelection selection,
        TContext context)
    {
        context.Selection.Push(selection);
        return DefaultAction;
    }

    protected virtual ISelectionVisitorAction Leave(
        ISelection selection,
        TContext context)
    {
        context.Selection.Pop();
        return DefaultAction;
    }
}
