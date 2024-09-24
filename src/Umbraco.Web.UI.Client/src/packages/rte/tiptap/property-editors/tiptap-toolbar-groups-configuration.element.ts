import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, property, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbId } from '@umbraco-cms/backoffice/id';

type ToolbarButton = {
	alias: string;
	label: string;
	icon: string;
};

type ToolbarGroup = {
	groupId: string;
	buttons: ToolbarButton[];
};

type ToolbarRow = {
	rowId: string;
	groups: ToolbarGroup[];
};

type ToolbarConfig = ToolbarRow[];

let toolbarConfig: ToolbarConfig = [
	{
		rowId: 'asdasgasgasd',
		groups: [
			{
				groupId: 'asdasldjh12h123',
				buttons: [
					{ alias: 'bold', label: 'Bold', icon: 'bold-icon' },
					{ alias: 'italic', label: 'Italic', icon: 'italic-icon' },
				],
			},
			{
				groupId: 'askdjljk123ljkh12kj3h',
				buttons: [{ alias: 'underline', label: 'Underline', icon: 'underline-icon' }],
			},
		],
	},
	{
		rowId: '1l2j3ljk123l21j3',
		groups: [
			{
				groupId: 'asdashd9ashd0as87hdoasudh',
				buttons: [{ alias: 'align-left', label: 'Align Left', icon: 'align-left-icon' }],
			},
		],
	},
];

@customElement('umb-tiptap-toolbar-groups-configuration')
export class UmbTiptapToolbarGroupsConfigurationElement extends UmbLitElement {
	@property({ attribute: false })
	set value(value: string) {
		this.#value = value as string;

		this.requestUpdate('#selectedValuesNew');
	}
	get value(): string {
		return this.#value;
	}

	#value = '';

	#addGroup = (rowId: string) => {
		const row = toolbarConfig.find((row) => row.rowId === rowId);
		if (!row) return;

		row.groups.push({
			groupId: UmbId.new(),
			buttons: [],
		});

		this.requestUpdate();
	};

	#removeGroup(groupId: string) {
		const row = toolbarConfig.find((row) => row.groups.some((group) => group.groupId === groupId));
		if (!row) return;

		row.groups = row.groups.filter((group) => group.groupId !== groupId);

		this.requestUpdate();
	}

	#addRow = () => {
		toolbarConfig.push({
			rowId: UmbId.new(),
			groups: [
				{
					groupId: UmbId.new(),
					buttons: [],
				},
			],
		});

		this.requestUpdate();
	};

	#removeRow(rowId: string) {
		toolbarConfig = toolbarConfig.filter((row) => row.rowId !== rowId);

		this.requestUpdate();
	}

	#moveButton(alias: string, targetGroupId: string) {
		const sourceGroup = toolbarConfig
			.flatMap((row) => row.groups)
			.find((group) => group.buttons.some((button) => button.alias === alias));

		if (!sourceGroup) return;

		// remove button from source group
		const buttonIndex = sourceGroup.buttons.findIndex((button) => button.alias === alias);
		const button = sourceGroup.buttons.splice(buttonIndex, 1)[0];

		// add button to target group
		const targetGroup = toolbarConfig.flatMap((row) => row.groups).find((group) => group.groupId === targetGroupId);
		if (!targetGroup) return;

		targetGroup.buttons.push(button);

		this.requestUpdate();
	}

	#onDragStart = (event: DragEvent, alias: string) => {
		event.dataTransfer!.setData('text/plain', alias);
		event.dataTransfer!.dropEffect = 'move';
	};

	#onDragOver = (event: DragEvent) => {
		event.preventDefault();
	};

	#onDrop = (event: DragEvent, groupId: string) => {
		event.preventDefault();

		const alias = event.dataTransfer!.getData('text/plain');
		if (!alias) return;

		this.#moveButton(alias, groupId);
	};

	#renderButton = (button: ToolbarButton) => {
		return html`<button @dragstart=${(e: DragEvent) => this.#onDragStart(e, button.alias)} draggable="true">
			${button.label}
		</button>`;
	};

	#renderGroup = (group: ToolbarGroup) => {
		return html`<div
			class="group"
			dropzone="move"
			@dragover=${this.#onDragOver}
			@drop=${(e: DragEvent) => this.#onDrop(e, group.groupId)}>
			${repeat(group.buttons, (button) => button.alias, this.#renderButton)}
			<button class="remove-group-button" @click=${() => this.#removeGroup(group.groupId)}>X</button>
		</div>`;
	};

	#renderRow = (row: ToolbarRow) => {
		return html`<div class="row">
			${repeat(row.groups, (group) => group.groupId, this.#renderGroup)}<button
				@click=${() => this.#addGroup(row.rowId)}>
				Add group
			</button>
			<button class="remove-row-button" @click=${() => this.#removeRow(row.rowId)}>X</button>
		</div>`;
	};

	override render() {
		return html`<button @click=${this.#addGroup}>Add Group</button>
			<button @click=${this.#addRow}>Add Row</button>

			${repeat(toolbarConfig, (row) => row.rowId, this.#renderRow)}`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: 6px;
			}
			.row {
				position: relative;
				display: flex;
				gap: 12px;
			}
			.group {
				position: relative;
				display: flex;
				gap: 3px;
				border: 1px solid #ccc;
				padding: 6px;
				min-height: 24px;
				min-width: 24px;
			}

			.remove-group-button {
				position: absolute;
				top: -4px;
				right: -4px;
				display: none;
			}
			.group:hover .remove-group-button {
				display: block;
			}

			.remove-row-button {
				position: absolute;
				left: -25px;
				top: 8px;
				display: none;
			}
			.row:hover .remove-row-button {
				display: block;
			}
		`,
	];
}

export default UmbTiptapToolbarGroupsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-groups-configuration': UmbTiptapToolbarGroupsConfigurationElement;
	}
}
