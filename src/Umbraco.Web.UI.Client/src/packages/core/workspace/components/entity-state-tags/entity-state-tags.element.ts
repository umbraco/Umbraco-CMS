import type { UmbEntityStateEntry, UmbEntityStateLook } from '@umbraco-cms/backoffice/entity-state';
import { css, customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

// The one place a semantic `look` word becomes an actual UUI color token.
const LOOK_TO_UUI_COLOR: Record<UmbEntityStateLook, UUIInterfaceColor> = {
	positive: 'positive',
	warning: 'warning',
	danger: 'danger',
	neutral: 'default',
};

/**
 * Renders a set of {@link UmbEntityStateEntry} as prominent tags, e.g. next to an entity's name.
 * @element umb-entity-state-tags
 */
@customElement('umb-entity-state-tags')
export class UmbEntityStateTagsElement extends UmbLitElement {
	@property({ type: Array })
	states: Array<UmbEntityStateEntry> = [];

	override render() {
		if (!this.states.length) return nothing;

		return this.states.map(
			(entry) => html`
				<uui-tag
					color=${entry.look ? LOOK_TO_UUI_COLOR[entry.look] : 'default'}
					look="secondary"
					title=${ifDefined(entry.detail ? this.localize.string(entry.detail) : undefined)}>
					${this.localize.string(entry.label ?? '')}
				</uui-tag>
			`,
		);
	}

	static override styles = [
		css`
			:host {
				display: contents;
			}

			uui-tag {
				font-size: 11px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-state-tags': UmbEntityStateTagsElement;
	}
}
