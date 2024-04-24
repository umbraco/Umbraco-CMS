import type { UmbInputDocumentElement } from '../../components/input-document/input-document.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-property-editor-ui-document-picker')
export class UmbPropertyEditorUIDocumentPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	public value?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const minMax = config.getValueByAlias<NumberRangeValueType>('validationLimit');
		this.min = minMax?.min ?? 0;
		this.max = minMax?.max ?? Infinity;

		this.ignoreUserStartNodes = config.getValueByAlias('ignoreUserStartNodes') ?? false;
		this.startNodeId = config.getValueByAlias('startNodeId');
		this.showOpenButton = config.getValueByAlias('showOpenButton') ?? false;
	}

	@state()
	min = 0;

	@state()
	max = Infinity;

	@state()
	startNodeId?: string;

	@state()
	showOpenButton?: boolean;

	@state()
	ignoreUserStartNodes?: boolean;

	#onChange(event: CustomEvent & { target: UmbInputDocumentElement }) {
		this.value = event.target.selection.join(',');
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-document
				.min=${this.min}
				.max=${this.max}
				.startNodeId=${this.startNodeId ?? ''}
				.value=${this.value ?? ''}
				?showOpenButton=${this.showOpenButton}
				?ignoreUserStartNodes=${this.ignoreUserStartNodes}
				@change=${this.#onChange}>
			</umb-input-document>
		`;
	}
}

export default UmbPropertyEditorUIDocumentPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-document-picker': UmbPropertyEditorUIDocumentPickerElement;
	}
}
