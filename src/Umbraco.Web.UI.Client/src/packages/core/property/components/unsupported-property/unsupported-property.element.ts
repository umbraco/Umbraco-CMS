import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

/**
 *  @description Component for displaying an unsupported property
 */

@customElement('umb-unsupported-property')
export class UmbUnsupportedPropertyElement extends UmbLitElement {
	@property({ type: String })
	public alias = '';

	@property({ type: String })
	public schema = '';

	override render() {
		return html`<div id="not-supported">
			<umb-localize key="blockEditor_propertyEditorNotSupported" .args=${[this.alias, this.schema]}></umb-localize>
		</div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1) 0;
			}

			#not-supported {
				background-color: var(--uui-color-danger);
				color: var(--uui-color-surface);
				padding: var(--uui-size-space-4);
				border-radius: var(--uui-border-radius);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-unsupported-property': UmbUnsupportedPropertyElement;
	}
}
