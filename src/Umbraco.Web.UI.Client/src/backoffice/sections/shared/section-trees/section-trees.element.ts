import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, Subscription, switchMap, EMPTY, of } from 'rxjs';

import { UmbExtensionRegistry } from '../../../../core/extension';
import { UmbSectionContext } from '../../section.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

import '../../../trees/shared/tree-extension.element';

@customElement('umb-section-trees')
export class UmbSectionTreesElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles];

	@state()
	private _treeAliases: Array<string> = [];

	private _extensionStore?: UmbExtensionRegistry;
	private _treesSubscription?: Subscription;

	private _sectionContext?: UmbSectionContext;
	private _sectionContextSubscription?: Subscription;

	constructor() {
		super();

		this.consumeAllContexts(['umbExtensionRegistry', 'umbSectionContext'], (instances) => {
			this._extensionStore = instances['umbExtensionRegistry'];
			this._sectionContext = instances['umbSectionContext'];
			this._useTrees();
		});
	}

	private _useTrees() {
		if (!this._extensionStore || !this._sectionContext) return;

		this._treesSubscription?.unsubscribe();

		this._treesSubscription = this._sectionContext?.data
			.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;

					return (
						this._extensionStore
							?.extensionsOfType('tree')
							.pipe(
								map((trees) =>
									trees.filter((tree) => tree.meta.sections.includes(section.alias)).map((tree) => tree.alias)
								)
							) ?? of([])
					);
				})
			)
			.subscribe((treeAliases) => {
				this._treeAliases = treeAliases;
			});
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this._treesSubscription?.unsubscribe();
		this._sectionContextSubscription?.unsubscribe();
	}

	render() {
		return html`${this._treeAliases.map((treeAlias) => html`<umb-tree alias="${treeAlias}"></umb-tree>`)} `;
	}
}

export default UmbSectionTreesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-trees': UmbSectionTreesElement;
	}
}
