import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbDataTypeStore } from '../../../core/stores/data-type.store';
import { map, Subscription, first } from 'rxjs';
import { UmbDocumentTypeStore } from '../../../core/stores/document-type.store';
import { UmbExtensionRegistry } from '../../../core/extension';
import '../../tree/shared/tree.element';
import { UmbSectionContext } from '../section.context';

@customElement('umb-settings-section-tree')
class UmbSettingsSectionTree extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles];

	// TODO: implement dynamic tree data
	@state()
	_dataTypes: Array<any> = [];

	@state()
	_documentTypes: Array<any> = [];

	@state()
	private _trees: Array<any> = [];

	@state()
	private _currentSectionAlias?: string;

	private _dataTypeStore?: UmbDataTypeStore;
	private _documentTypeStore?: UmbDocumentTypeStore;

	private _dataTypesSubscription?: Subscription;
	private _documentTypesSubscription?: Subscription;

	private _extensionStore?: UmbExtensionRegistry;
	private _treeSubscription?: Subscription;

	private _sectionContextSubscription?: Subscription;
	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext('umbSectionContext', (context: UmbSectionContext) => {
			this._sectionContext = context;
			this._useSectionContext();
		});

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

		this.consumeContext('umbExtensionRegistry', (store) => {
			this._extensionStore = store;
			this._useTrees();
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();

		this._dataTypesSubscription?.unsubscribe();
		this._documentTypesSubscription?.unsubscribe();
	}

	private _useSectionContext() {
		this._sectionContextSubscription?.unsubscribe();

		this._sectionContextSubscription = this._sectionContext?.data.pipe(first()).subscribe((section) => {
			this._currentSectionAlias = section.alias;
		});
	}

	private _useTrees() {
		//TODO: Merge streams
		if (this._extensionStore && this._currentSectionAlias) {
			this._treeSubscription?.unsubscribe();

			this._treeSubscription = this._extensionStore
				?.extensionsOfType('tree')
				.pipe(
					map((extensions) =>
						extensions
							.filter((extension) => extension.meta.sections.includes(this._currentSectionAlias as string)) // TODO: Why do whe need "as string" here??
							.sort((a, b) => b.meta.weight - a.meta.weight)
					)
				)
				.subscribe((treeExtensions) => {
					this._trees = treeExtensions;
					console.log('Wrosk', this._trees);
				});
		}
	}

	render() {
		return html`
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

			${this._trees.map((tree) => html`<umb-tree .tree=${tree}></umb-tree>`)}
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-settings-section-tree': UmbSettingsSectionTree;
	}
}
