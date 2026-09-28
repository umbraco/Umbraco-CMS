import { css, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { LitElement } from '@umbraco-cms/backoffice/external/lit';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';

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
		/**
		 * Whether this ref links to a workspace and should show the entity frame on hover/focus.
		 * Refs that never link (e.g. a fallback or otherwise non-navigable ref) should leave this at its default.
		 * @protected
		 * @returns {boolean} Whether the entity frame should be shown.
		 */
		protected get isEntityFrameNavigable(): boolean {
			return false;
		}

		/**
		 * Renders the entity frame, when {@link isEntityFrameNavigable} is `true`.
		 * @protected
		 * @param {string} [name] - The entity name shown in the frame's tab.
		 * @returns {unknown} The entity frame template, or `nothing` when not navigable.
		 */
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

	return UmbEntityItemRefFrameMixinClass;
};
