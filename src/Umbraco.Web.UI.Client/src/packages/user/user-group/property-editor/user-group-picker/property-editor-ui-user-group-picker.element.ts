import type { UmbUserGroupInputElement } from '../../components/index.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNumberRangeValueType, UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-user-group-picker')
export class UmbUserGroupPickerPropertyEditorUIElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ attribute: false })
	public value: Array<UmbReferenceByUnique> = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const minMax = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;
	}

	@state()
	private _min = 0;

	@state()
	private _max = Infinity;

	#onChange(event: CustomEvent & { target: UmbUserGroupInputElement }) {
		this.value = event.target.selection.map((unique) => ({ unique }));
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-user-group-input
				.selection=${this.value.map((reference) => reference.unique)}
				.min=${this._min}
				.max=${this._max}
				@change=${this.#onChange}>
			</umb-user-group-input>
		`;
	}
}

export { UmbUserGroupPickerPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-user-group-picker': UmbUserGroupPickerPropertyEditorUIElement;
	}
}
