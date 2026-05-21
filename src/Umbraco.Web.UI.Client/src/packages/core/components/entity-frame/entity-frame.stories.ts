import { html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityFrameElement } from './entity-frame.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';

import './entity-frame.element.js';

const meta: Meta<UmbEntityFrameElement> = {
	component: 'umb-entity-frame',
	title: 'Generic Components/Entity Frame',
	args: {
		label: 'Document: Hero Banner',
	},
	decorators: [(story) => html`<div style="padding: 1.75rem; max-width: 600px;">${story()}</div>`],
	render: (args) => html`
		<div
			style="position: relative; width: 240px; height: 120px; padding: 16px; border: 1px dashed rgba(0,0,0,0.2); border-radius: var(--uui-border-radius);">
			<p>Entity label tab with full opacity.</p>
			<umb-entity-frame .label=${args.label}></umb-entity-frame>
		</div>
	`,
};

export default meta;
type Story = StoryObj<UmbEntityFrameElement>;

export const Docs: Story = {};

export const OnHover: Story = {
	render: (args) => html`
		<style>
			.demo-on-hover {
				--umb-entity-frame-opacity: 0;
				position: relative;
				width: 240px;
				height: 120px;
				padding: 16px;
				border: 1px dashed rgba(0, 0, 0, 0.2);
				border-radius: var(--uui-border-radius);
			}
			.demo-on-hover:hover {
				--umb-entity-frame-opacity: 1;
			}
		</style>
		<div class="demo-on-hover">
			<p>Hover this container to reveal.</p>
			<umb-entity-frame .label=${args.label}></umb-entity-frame>
		</div>
	`,
};

export const OnHoverOrFocus: Story = {
	render: (args) => html`
		<style>
			.demo-hover-focus {
				--umb-entity-frame-opacity: 0;
				position: relative;
				width: 240px;
				height: 120px;
				padding: 16px;
				border: 1px dashed rgba(0, 0, 0, 0.2);
				border-radius: var(--uui-border-radius);
			}
			.demo-hover-focus:hover,
			.demo-hover-focus:focus-within {
				--umb-entity-frame-opacity: 1;
			}
		</style>
		<div class="demo-hover-focus">
			<p>Hover, or Tab into the button below.</p>
			<uui-button look="primary" color="default">Focusable child</uui-button>
			<umb-entity-frame .label=${args.label}></umb-entity-frame>
		</div>
	`,
};

export const WithSlot: Story = {
	render: () => html`
		<div
			style="position: relative; width: 240px; height: 120px; padding: 16px; border: 1px dashed rgba(0,0,0,0.2); border-radius: var(--uui-border-radius);">
			<p>Slotted content overrides the label.</p>
			<umb-entity-frame>
				<uui-icon name="icon-document"></uui-icon>
				<span><strong>Document</strong>: Hero Banner</span>
			</umb-entity-frame>
		</div>
	`,
};

export const WrappingButton: Story = {
	render: () => html`
		<style>
			.demo-wrapping-button {
				--umb-entity-frame-opacity: 0;

				position: relative;
				display: inline-block;
			}
			.demo-wrapping-button:hover {
				--umb-entity-frame-opacity: 1;
			}
		</style>
		<div class="demo-wrapping-button">
			<uui-button look="primary">
				Edit Hero Banner
				<umb-entity-frame label="uui-button"></umb-entity-frame>
			</uui-button>
		</div>
	`,
};

export const Nested: Story = {
	render: () => html`
		<style>
			.demo-nested-outer {
				--umb-entity-frame-opacity: 0;

				position: relative;
				width: 320px;
				height: 200px;
				padding: 32px;
				border: 1px dashed rgba(0, 0, 0, 0.2);
				border-radius: var(--uui-border-radius);
			}
			.demo-nested-outer:hover {
				--umb-entity-frame-opacity: 1;
			}
			.demo-nested-inner {
				--umb-entity-frame-opacity: 0;

				position: relative;
				margin-top: 16px;
				width: 200px;
				height: 80px;
				padding: 16px;
				border: 1px dashed rgba(0, 0, 0, 0.2);
				border-radius: var(--uui-border-radius);
			}
			.demo-nested-inner:hover {
				--umb-entity-frame-opacity: 1;
			}
		</style>
		<div class="demo-nested-outer">
			<p style="margin: 0;">Outer container (both visible when hovering inner)</p>
			<umb-entity-frame label="Outer"></umb-entity-frame>

			<div class="demo-nested-inner">
				<p style="margin: 0;">Inner container (only inner visible without hovering outer)</p>
				<umb-entity-frame label="Inner"></umb-entity-frame>
			</div>
		</div>
	`,
};

export const WithCustomColor: Story = {
	render: () => html`
		<div
			style="position: relative; width: 240px; height: 120px; padding: 16px; border: 1px dashed rgba(0,0,0,0.2); border-radius: var(--uui-border-radius); --umb-entity-frame-color: #7532c8;">
			<p style="margin: 0;">Themed via <code>--umb-entity-frame-color</code>.</p>
			<umb-entity-frame label="Custom"></umb-entity-frame>
		</div>
	`,
};

export const ReferenceList: Story = {
	render: () => {
		const items = [
			{ label: 'Home', color: 'maroon' },
			{ label: 'About', color: 'green' },
			{ label: 'Contact', color: 'blue' },
			{ label: 'Blog', color: 'purple' },
		];

		return html`
			<style>
				uui-ref-list {
					--umb-entity-frame-opacity: 0;
				}
				uui-ref-node:hover,
				uui-ref-node:focus-within {
					--umb-entity-frame-opacity: 1;
				}
			</style>
			<uui-ref-list>
				${items.map(
					(item) => html`
						<uui-ref-node name=${item.label} detail="path/to/nowhere" style="--umb-entity-frame-color: ${item.color}">
							<umb-entity-frame label=${item.label}></umb-entity-frame>
						</uui-ref-node>
					`,
				)}
			</uui-ref-list>
		`;
	},
};
