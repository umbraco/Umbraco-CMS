import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, Subscription, switchMap, EMPTY, of } from 'rxjs';

import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionRegistry } from '../../../core/extension';
import { UmbSectionContext } from '../section.context';

import '../../trees/shared/tree-extension.element';

@customElement('umb-section-trees')
export class UmbSectionTrees extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles];

	@state()
	private _treeAliases: Array<string> = [];

	private _extensionStore?: UmbExtensionRegistry;
	private _treesSubscription?: Subscription;

	private _sectionContext?: UmbSectionContext;
	private _sectionContextSubscription?: Subscription;

	constructor() {
		super();

		// TODO: wait for more contexts
		this.consumeContext('umbExtensionRegistry', (extensionStore: UmbExtensionRegistry) => {
			this._extensionStore = extensionStore;
			this._useTrees();
		});

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
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
						this._extensionStore?.extensionsOfType('tree').pipe(
							map((trees) =>
								trees
									.filter((tree) => tree.meta.sections.includes(section.alias))
									.sort((a, b) => b.meta.weight - a.meta.weight)
									.map((tree) => tree.alias)
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

export default UmbSectionTrees;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-trees': UmbSectionTrees;
	}
}
