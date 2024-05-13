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
		if (minMax) {
			this._min = minMax.min && minMax.min > 0 ? minMax.min : 0;
			this._max = minMax.max && minMax.max > 0 ? minMax.max : Infinity;
		}

		this._ignoreUserStartNodes = config.getValueByAlias('ignoreUserStartNodes') ?? false;
		this._startNodeId = config.getValueByAlias('startNodeId');
		this._showOpenButton = config.getValueByAlias('showOpenButton') ?? false;
	}

	@state()
	private _min = 0;

	@state()
	private _max = Infinity;

	@state()
	private _startNodeId?: string;

	@state()
	private _showOpenButton?: boolean;

	@state()
	private _ignoreUserStartNodes?: boolean;

	#onChange(event: CustomEvent & { target: UmbInputDocumentElement }) {
		this.value = event.target.selection.join(',');
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		const startNode = this._startNodeId ? { unique: this._startNodeId } : undefined;
		return html`
			<umb-input-document
				.min=${this._min}
				.max=${this._max}
				.startNode=${startNode}
				.value=${this.value ?? ''}
				?ignoreUserStartNodes=${this._ignoreUserStartNodes}
				?showOpenButton=${this._showOpenButton}
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
