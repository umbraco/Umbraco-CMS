import { css, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { CSSResultGroup, LitElement } from '@umbraco-cms/backoffice/external/lit';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';

/**
 * Instance shape added by {@link UmbEntityItemRefFrameMixin}, declared separately so it can be named in the
 * emitted `.d.ts` (an anonymous class expression can't describe its protected members there).
 * @mixin
 */
export declare abstract class UmbEntityItemRefFrameMixinElement extends LitElement {
	/**
	 * Whether this ref links to a workspace and should show the entity frame on hover/focus.
	 * Refs that never link should leave this at its default.
	 * @protected
	 * @returns {boolean} Whether the entity frame should be shown.
	 */
	protected get isEntityFrameNavigable(): boolean;

	/**
	 * Renders the entity frame, when {@link isEntityFrameNavigable} is `true`.
	 * @protected
	 * @param {string} [name] - The entity name shown in the frame's tab.
	 * @returns {unknown} The entity frame template, or `nothing` when not navigable.
	 */
	protected renderEntityFrame(name?: string): unknown;
}

// Constructor type carries `styles` separately, since ClassConstructor<T> only describes instances, not statics.
type UmbEntityItemRefFrameMixinConstructor = ClassConstructor<UmbEntityItemRefFrameMixinElement> & {
	styles: CSSResultGroup[];
};

/**
 * Adds the shared border-and-tab entity frame — shown on hover/focus via `<umb-entity-frame>` — to an entity
 * item ref element.
 * @function UmbEntityItemRefFrameMixin
 * @param {ClassConstructor<LitElement>} superClass - The class to mix the entity frame behaviour into.
 * @returns {ClassConstructor<LitElement>} The mixed-in class.
 * @mixin
 */
export const UmbEntityItemRefFrameMixin = <T extends ClassConstructor<LitElement>>(superClass: T) => {
	class UmbEntityItemRefFrameMixinClass extends superClass {
		protected get isEntityFrameNavigable(): boolean {
			return false;
		}

		protected renderEntityFrame(name?: string) {
			if (!this.isEntityFrameNavigable) return nothing;
			return html`<umb-entity-frame aria-hidden="true"><uui-icon name="link"></uui-icon> ${name}</umb-entity-frame>`;
		}

		static styles = [
			css`
				:host {
					--umb-entity-frame-opacity: 0;
					--umb-entity-frame-color: var(--umb-color-reference);
					--umb-entity-frame-contrast-color: var(--umb-color-reference-contrast);

					display: block;
					position: relative;
				}

				:host(:hover),
				:host(:focus-within) {
					--umb-entity-frame-opacity: 1;
				}
			`,
		];
	}

	return UmbEntityItemRefFrameMixinClass as unknown as UmbEntityItemRefFrameMixinConstructor & T;
};
