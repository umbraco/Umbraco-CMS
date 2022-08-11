import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { data as dataTypeData } from '../../../mocks/data/data-type.data';
import { data as documentTypeData } from '../../../mocks/data/document-type.data';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbDataTypeStore } from '../../../core/stores/data-type.store';
import { Subscription } from 'rxjs';
import { UmbDocumentTypeStore } from '../../../core/stores/document-type.store';

@customElement('umb-settings-section-tree')
class UmbSettingsSectionTree extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			h3 {
				padding: var(--uui-size-4) var(--uui-size-8);
			}
		`,
	];

	// TODO: implement dynamic tree data
	@state()
	_dataTypes: Array<any> = [];

	@state()
	_documentTypes: Array<any> = [];

	private _dataTypeStore?: UmbDataTypeStore;
	private _documentTypeStore?: UmbDocumentTypeStore;

	private _dataTypesSubscription?: Subscription;
	private _documentTypesSubscription?: Subscription;

	constructor() {
		super();

		// TODO: temp solution until we know where to get tree data from
		this.consumeContext('umbDataTypeStore', (store) => {
			this._dataTypeStore = store;

			this._dataTypesSubscription = this._dataTypeStore?.getAll().subscribe((dataTypes) => {
				this._dataTypes = dataTypes;
			});
		});

		// TODO: temp solution until we know where to get tree data from
		this.consumeContext('umbDocumentTypeStore', (store) => {
			this._documentTypeStore = store;

			this._documentTypesSubscription = this._documentTypeStore?.getAll().subscribe((documentTypes) => {
				this._documentTypes = documentTypes;
			});
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._dataTypesSubscription?.unsubscribe();
		this._documentTypesSubscription?.unsubscribe();
	}

	render() {
		return html`
			<a href="${'/section/settings'}">
				<h3>Settings</h3>
			</a>

			<!-- TODO: hardcoded tree items. These should come the extensions -->
			<uui-menu-item label="Extensions" href="/section/settings/extensions"></uui-menu-item>
			<uui-menu-item label="Data Types" has-children>
				${this._dataTypes.map(
					(dataType) => html`
						<uui-menu-item label="${dataType.name}" href="/section/settings/data-type/${dataType.id}"></uui-menu-item>
					`
				)}
			</uui-menu-item>
			<uui-menu-item label="Document Types" has-children>
				${this._documentTypes.map(
					(documentType) => html`
						<uui-menu-item
							label="${documentType.name}"
							href="/section/settings/document-type/${documentType.id}"></uui-menu-item>
					`
				)}
			</uui-menu-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-settings-section-tree': UmbSettingsSectionTree;
	}
}
