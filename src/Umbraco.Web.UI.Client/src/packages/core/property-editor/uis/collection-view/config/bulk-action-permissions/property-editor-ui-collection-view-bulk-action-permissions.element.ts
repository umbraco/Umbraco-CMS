import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import {
	UmbPropertyEditorConfigCollection,
	UmbPropertyValueChangeEvent,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

type BulkActionPermissionType =
	| 'allowBulkCopy'
	| 'allowBulkDelete'
	| 'allowBulkMove'
	| 'allowBulkPublish'
	| 'allowBulkUnpublish';

interface BulkActionPermissions {
	allowBulkCopy?: boolean;
	allowBulkDelete?: boolean;
	allowBulkMove?: boolean;
	allowBulkPublish?: boolean;
	allowBulkUnpublish?: boolean;
}

/**
 * @element umb-property-editor-ui-collection-view-bulk-action-permissions
 */
@customElement('umb-property-editor-ui-collection-view-bulk-action-permissions')
export class UmbPropertyEditorUICollectionViewBulkActionPermissionsElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Object })
	value: BulkActionPermissions = {};

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(e: UUIBooleanInputEvent, type: BulkActionPermissionType) {
		switch (type) {
			case 'allowBulkPublish':
				this.value.allowBulkPublish = e.target.checked;
				break;
			case 'allowBulkUnpublish':
				this.value.allowBulkUnpublish = e.target.checked;
				break;
			case 'allowBulkMove':
				this.value.allowBulkMove = e.target.checked;
				break;
			case 'allowBulkCopy':
				this.value.allowBulkCopy = e.target.checked;
				break;
			case 'allowBulkDelete':
				this.value.allowBulkDelete = e.target.checked;
				break;
		}
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<uui-checkbox
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkPublish')}
				label="Allow bulk publish (content only)"></uui-checkbox>
			<uui-checkbox
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkUnpublish')}
				label="Allow bulk unpublish (content only)"></uui-checkbox>
			<uui-checkbox
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkCopy')}
				label="Allow bulk copy (content only)"></uui-checkbox>
			<uui-checkbox
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkMove')}
				label="Allow bulk move"></uui-checkbox>
			<uui-checkbox
				@change=${(e: UUIBooleanInputEvent) => this.#onChange(e, 'allowBulkDelete')}
				label="Allow bulk delete"></uui-checkbox> `;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUICollectionViewBulkActionPermissionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-bulk-action-permissions': UmbPropertyEditorUICollectionViewBulkActionPermissionsElement;
	}
}
