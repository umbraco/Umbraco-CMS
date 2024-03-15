import type { UmbDocumentTreeItemModel, UmbDocumentTreeItemVariantModel } from '../../tree/index.js';
import { UmbDocumentTreeRepository } from '../../tree/index.js';
import type { UmbDocumentVariantModel } from '../../types.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbVariantableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbVariantModel } from '@umbraco-cms/backoffice/variant';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

@customElement('umb-variant-workspace-breadcrumb')
export class UmbVariantWorkspaceBreadcrumbElement extends UmbLitElement {
	#workspaceContext?: UmbVariantableWorkspaceContextInterface<UmbVariantModel>;
	#treeRepository = new UmbDocumentTreeRepository(this);

	@state()
	_isNew = false;

	@state()
	_name: string = '';

	@state()
	_ancestors: UmbDocumentTreeItemModel[] = [];

	@state()
	_activeVariantId?: UmbVariantId;

	constructor() {
		super();
		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestAncestors();
			this.#observeActiveVariant();
		});
	}

	#observeActiveVariant() {
		this.observe(
			this.#workspaceContext?.splitView.activeVariantsInfo,
			(value) => {
				if (!value) return;
				this._activeVariantId = UmbVariantId.Create(value[0]);
				this.#observeActiveVariantName();
			},

			'breadcrumbWorkspaceActiveVariantObserver',
		);
	}

	#observeActiveVariantName() {
		this.observe(
			this.#workspaceContext?.name(this._activeVariantId),
			(value) => (this._name = value || 'Untitled'),
			'breadcrumbWorkspaceNameObserver',
		);
	}

	#getAncestorVariantName(variants: Array<UmbDocumentTreeItemVariantModel> = []) {
		return variants.find((x) => this._activeVariantId.compare(x))?.name;
	}

	async #requestAncestors() {
		const unique = this.#workspaceContext?.getUnique();
		if (!unique) throw new Error('Unique is not available');
		const { data } = await this.#treeRepository.requestTreeItemAncestors({ descendantUnique: unique });

		if (data) {
			this._ancestors = data;
		}
	}

	render() {
		return html`
			<uui-breadcrumbs>
				${this._ancestors.map(
					(item) =>
						html`<uui-breadcrumb-item href="/section/content/workspace/document/edit/${item.unique}"
							>${this.#getAncestorVariantName(item.variants)}</uui-breadcrumb-item
						>`,
				)}
				<uui-breadcrumb-item>${this._name}</uui-breadcrumb-item>
			</uui-breadcrumbs>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbVariantWorkspaceBreadcrumbElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-variant-workspace-breadcrumb': UmbVariantWorkspaceBreadcrumbElement;
	}
}
