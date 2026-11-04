import { umbOrderActionsByGroup } from '@umbraco-cms/backoffice/action';
import type { ManifestActionGroupBase, UmbActionGroupBlock, UmbGroupableAction } from '@umbraco-cms/backoffice/action';
import { css, customElement, html, ifDefined, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * An action rendered by `<umb-grouped-action-list>`: a groupable action and its element.
 */
export interface UmbGroupedActionListItem extends UmbGroupableAction {
	component?: HTMLElement;
}

/**
 * @element umb-grouped-action-list
 * @description Renders the actions of a menu in blocks: one per registered action group, ordered by the group's
 * weight, then one with the actions that have no registered group. A line separates the blocks, and an action with
 * `separatorBefore` starts a new line inside its block. A group is exposed to assistive technology by its label.
 */
@customElement('umb-grouped-action-list')
export class UmbGroupedActionListElement extends UmbLitElement {
	/**
	 * The extension type of the action groups, for example `entityActionGroup`.
	 * @attr group-type
	 */
	@property({ attribute: 'group-type' })
	groupType?: string;

	/**
	 * The visible actions with their elements, in any order.
	 */
	@property({ attribute: false })
	actions: Array<UmbGroupedActionListItem> = [];

	@state()
	private _groups: Array<ManifestActionGroupBase> = [];

	#blocks: Array<UmbActionGroupBlock<UmbGroupedActionListItem>> = [];

	override focus(options?: FocusOptions) {
		this.#blocks[0]?.actions[0]?.component?.focus(options);
	}

	protected override willUpdate(changedProperties: PropertyValues) {
		super.willUpdate(changedProperties);
		if (changedProperties.has('groupType')) {
			this.#observeGroups();
		}
		if (changedProperties.has('actions') || changedProperties.has('_groups')) {
			this.#blocks = umbOrderActionsByGroup(this.actions, this._groups);
		}
	}

	#observeGroups() {
		if (!this.groupType) {
			this.removeUmbControllerByAlias('_observeGroups');
			this._groups = [];
			return;
		}
		this.observe(
			umbExtensionsRegistry.byType(this.groupType),
			(groups) => (this._groups = groups as Array<ManifestActionGroupBase>),
			'_observeGroups',
		);
	}

	override render() {
		// The block of actions without a group only needs a name when other blocks surround it.
		const nameOtherBlock = this.#blocks.length > 1;
		return repeat(
			this.#blocks,
			(block) => block.group?.alias,
			(block) => {
				const named = !!block.group || nameOtherBlock;
				const label = block.group
					? this.localize.string(block.group.meta.label)
					: this.localize.term('actionGroups_other');
				return html`<div
					class="block"
					role=${ifDefined(named ? 'group' : undefined)}
					aria-label=${ifDefined(named ? label : undefined)}
					data-mark=${ifDefined(block.group ? `action-group:${block.group.alias}` : undefined)}>
					${this.#renderActions(block.actions)}
				</div>`;
			},
		);
	}

	#renderActions(actions: Array<UmbGroupedActionListItem>) {
		const segments = actions.reduce<Array<Array<UmbGroupedActionListItem>>>(
			(segments, action, index) => {
				if (index > 0 && action.manifest?.separatorBefore) segments.push([]);
				segments[segments.length - 1].push(action);
				return segments;
			},
			[[]],
		);
		if (segments.length === 1) return this.#renderSegment(actions);
		return repeat(
			segments,
			(segment) => segment[0].alias,
			(segment) => html`<div class="segment">${this.#renderSegment(segment)}</div>`,
		);
	}

	#renderSegment(actions: Array<UmbGroupedActionListItem>) {
		return repeat(
			actions,
			(action) => action.alias,
			(action) => action.component,
		);
	}

	static override styles = [
		css`
			:host {
				display: block;
			}

			.block + .block::before,
			.segment + .segment::before {
				content: '';
				display: block;
				border-top: 1px solid var(--uui-color-divider-standalone);
				margin: var(--uui-size-space-2) var(--uui-size-space-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-grouped-action-list': UmbGroupedActionListElement;
	}
}
