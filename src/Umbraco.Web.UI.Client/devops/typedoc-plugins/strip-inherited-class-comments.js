import { Converter, ReflectionKind } from 'typedoc';

/**
 * Strips class-level comments that TypeDoc copied from a base class.
 *
 * TypeDoc's default behaviour: when a subclass has no JSDoc of its own, its docs page
 * shows the nearest ancestor's class comment. For us that means every UmbLitElement
 * descendant displays "The base class for all Umbraco LitElement elements." until we
 * write a real one-liner per class.
 *
 * The inheritance actually happens in two places:
 *  1. comment discovery walks `extends` chains and sets `inheritedFromParentDeclaration`.
 *  2. `ImplementsPlugin.handleInheritedComments` clones the parent comment onto the
 *     child at EVENT_RESOLVE_END — and `Comment.clone()` carries the parent's
 *     `sourcePath` over but resets `inheritedFromParentDeclaration` to the parent's
 *     value (usually false, since the parent authored its own comment).
 *
 * So the only reliable signal is to compare `comment.sourcePath` to the reflection's
 * own source file. If they differ, the comment was authored on a different declaration
 * and we drop it from this class. Inherited member comments (methods, properties) are
 * intentionally left alone — those are usually useful.
 *
 * Known limitation: only `sources[0]` is checked as the "own" file. Classes that span
 * multiple declarations (e.g. declaration merging) where the comment lives on a
 * secondary declaration would be incorrectly stripped. We don't merge class
 * declarations anywhere in this codebase, so the risk is theoretical.
 *
 * Runs at EVENT_END so it sees the final state after both inheritance phases.
 * @param {import('typedoc').Application} app - the TypeDoc application instance
 */
export function load(app) {
	app.converter.on(Converter.EVENT_END, (context) => {
		const classes = context.project.getReflectionsByKind(ReflectionKind.Class);
		let stripped = 0;
		for (const reflection of classes) {
			const comment = reflection.comment;
			if (!comment) continue;

			const ownSource = reflection.sources?.[0]?.fileName;
			const commentSource = comment.sourcePath;
			if (!ownSource || !commentSource) continue;

			// Both paths are repo-relative; one may be the suffix of the other depending
			// on how TypeDoc resolved them, so check both directions.
			if (!commentSource.endsWith(ownSource) && !ownSource.endsWith(commentSource)) {
				reflection.comment = undefined;
				stripped++;
			}
		}
		app.logger.info(
			`strip-inherited-class-comments: stripped ${stripped} of ${classes.length} class comments inherited from a base class`,
		);
	});
}
