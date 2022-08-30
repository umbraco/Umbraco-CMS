import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map, Subscription, switchMap, EMPTY, of } from 'rxjs';

import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbExtensionRegistry } from '../../../core/extension';
import { UmbSectionContext } from '../section.context';
import type { ManifestTree } from '../../../core/models';

import '../../tree/shared/tree.element';

@customElement('umb-section-trees')
export class UmbSectionTrees extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles];

	@state()
	private _trees: Array<ManifestTree> = [];

	private _extensionStore?: UmbExtensionRegistry;
	private _treesSubscription?: Subscription;

	private _sectionContext?: UmbSectionContext;
	private _sectionContextSubscription?: Subscription;

	constructor() {
		super();

		// TODO: wait for more contexts
		this.consumeContext('umbExtensionRegistry', (extensionStore: UmbExtensionRegistry) => {
			this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
				this._extensionStore = extensionStore;
				this._sectionContext = sectionContext;
				this._useTrees();
			});
		});
	}

	private _useTrees() {
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
									trees
										.filter((tree) => tree.meta.sections.includes(section.alias))
										.sort((a, b) => b.meta.weight - a.meta.weight)
								)
							) ?? of([])
					);
				})
			)
			.subscribe((trees) => {
				this._trees = trees;
			});
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this._treesSubscription?.unsubscribe();
		this._sectionContextSubscription?.unsubscribe();
	}

	render() {
		return html`${this._trees.map((tree) => html`<umb-tree .tree=${tree}></umb-tree>`)} `;
	}
}

export default UmbSectionTrees;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-trees': UmbSectionTrees;
	}
}
