import type { UmbCollectionBulkActionPermissions } from '@umbraco-cms/backoffice/collection';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

type BulkActionPermissionType =
	| 'allowBulkCopy'
	| 'allowBulkDelete'
	| 'allowBulkMove'
	| 'allowBulkPublish'
	| 'allowBulkUnpublish';

/**
 * @element umb-property-editor-ui-collection-permissions
 * @deprecated No longer used internally. This will be removed in Umbraco 17. [LK]
 */
@customElement('umb-property-editor-ui-collection-permissions')
export class UmbPropertyEditorUICollectionPermissionsElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	private _value: UmbCollectionBulkActionPermissions = {
		allowBulkPublish: false,
		allowBulkUnpublish: false,
		allowBulkCopy: false,
		allowBulkDelete: false,
		allowBulkMove: false,
	};

	@property({ type: Object })
	public set value(obj: UmbCollectionBulkActionPermissions) {
		if (!obj) return;
		this._value = obj;
	}
	public get value() {
		return this._value;
	}

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(e: UUIBooleanInputEvent, type: BulkActionPermissionType) {
		switch (type) {
			case 'allowBulkPublish':
				this.value = { ...this.value, allowBulkPublish: e.target.checked };
				break;
			case 'allowBulkUnpublish':
				this.value = { ...this.value, allowBulkUnpublish: e.target.checked };
				break;
			case 'allowBulkMove':
				this.value = { ...this.value, allowBulkMove: e.target.checked };
				break;
			case 'allowBulkCopy':
				this.value = { ...this.value, allowBulkCopy: e.target.checked };
				break;
			case 'allowBulkDelete':
				this.value = { ...this.value, allowBulkDelete: e.target.checked };
				break;
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	protected override firstUpdated() {
		console.warn(
			'The `umb-property-editor-ui-collection-permissions` component has been deprecated, it will be removed in Umbraco 17.',
		);
	}

	override render() {
		return html`<uui-toggle
				?checked=${this.value.allowBulkPublish}
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkPublish')}
				label="Allow bulk publish (content only)"></uui-toggle>
			<uui-toggle
				?checked=${this.value.allowBulkUnpublish}
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkUnpublish')}
				label="Allow bulk unpublish (content only)"></uui-toggle>
			<uui-toggle
				?checked=${this.value.allowBulkCopy}
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkCopy')}
				label="Allow bulk duplicate (content only)"></uui-toggle>
			<uui-toggle
				?checked=${this.value.allowBulkMove}
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkMove')}
				label="Allow bulk move"></uui-toggle>
			<uui-toggle
				?checked=${this.value.allowBulkDelete}
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkDelete')}
				label="Allow bulk trash"></uui-toggle>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export default UmbPropertyEditorUICollectionPermissionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-permissions': UmbPropertyEditorUICollectionPermissionsElement;
	}
}
