import { UmbSectionSidebarMenuElement } from '../section-sidebar-menu/section-sidebar-menu.element.js';
import type { ManifestSectionSidebarAppMenuWithEntityActionsKind } from '../section-sidebar-menu/types.js';
import { css, html, customElement, type PropertyValues, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbParentEntityContext } from '@umbraco-cms/backoffice/entity';

@customElement('umb-section-sidebar-menu-with-entity-actions')
export class UmbSectionSidebarMenuWithEntityActionsElement extends UmbSectionSidebarMenuElement<ManifestSectionSidebarAppMenuWithEntityActionsKind> {
	@state()
	private _unique = null;

	@state()
	private _entityType?: string | null;

	#parentContext = new UmbParentEntityContext(this);

	protected override updated(_changedProperties: PropertyValues<this>): void {
		if (_changedProperties.has('manifest')) {
			const entityType = this.manifest?.meta.entityType;
			this.#parentContext.setParent(entityType ? { unique: this._unique, entityType } : undefined);
		}
	}

	override renderHeader() {
		const label = this.localize.string(this.manifest?.meta?.label ?? '');
		return html`
			<div id="header">
				<h3>${label}</h3>
				<umb-entity-actions-bundle
					slot="actions"
					.unique=${this._unique}
					.entityType=${this.manifest?.meta.entityType}
					.label=${label}>
				</umb-entity-actions-bundle>
			</div>
		`;
	}

	static override styles = [
		...UmbSectionSidebarMenuElement.styles,
		css`
			#header {
				display: flex;
				flex-direction: row;
				align-items: center;
			}
			#header > :first-child {
				flex-grow: 1;
			}
		`,
	];
}

export default UmbSectionSidebarMenuWithEntityActionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-menu-with-entity-actions': UmbSectionSidebarMenuElement;
	}
}
