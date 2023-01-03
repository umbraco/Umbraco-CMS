import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, switchMap, EMPTY, of } from 'rxjs';

import { UmbSectionContext } from '../section.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import '../../tree/tree-extension.element';

@customElement('umb-section-trees')
export class UmbSectionTreesElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles];

	@state()
	private _treeAliases: Array<string> = [];

	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext('umbSectionContext', (instance) => {
			this._sectionContext = instance;
			this._observeTrees();
		});
	}

	private _observeTrees() {
		if (!this._sectionContext) return;

		this.observe<string[]>(
			this._sectionContext?.data.pipe(
				switchMap((section) => {
					if (!section) return EMPTY;

					return (
						umbExtensionsRegistry
							?.extensionsOfType('tree')
							.pipe(
								map((trees) =>
									trees.filter((tree) => tree.meta.sections.includes(section.alias)).map((tree) => tree.alias)
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

export default UmbSectionTreesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-trees': UmbSectionTreesElement;
	}
}
