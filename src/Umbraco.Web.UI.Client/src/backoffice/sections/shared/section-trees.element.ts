import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, switchMap, EMPTY, of } from 'rxjs';

import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionRegistry } from '../../../core/extension';
import { UmbSectionContext } from '../section.context';

import '../../trees/shared/tree-extension.element';
import { UmbObserverMixin } from '../../../core/observer';

@customElement('umb-section-trees')
export class UmbSectionTrees extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles];

	@state()
	private _treeAliases: Array<string> = [];

	private _extensionStore?: UmbExtensionRegistry;
	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		// TODO: wait for more contexts
		this.consumeContext('umbExtensionRegistry', (extensionStore: UmbExtensionRegistry) => {
			this._extensionStore = extensionStore;
			this._observeTrees();
		});

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._observeTrees();
		});
	}

	private _observeTrees() {
		if (!this._extensionStore || !this._sectionContext) return;

		this.observe<string[]>(
			this._sectionContext?.data.pipe(
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
			),
			(treeAliases) => {
				this._treeAliases = treeAliases;
			}
		);
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
