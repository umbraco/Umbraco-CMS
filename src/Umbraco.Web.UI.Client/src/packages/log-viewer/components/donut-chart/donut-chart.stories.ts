import type { UmbDonutChartElement } from './donut-chart.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html, ifDefined } from '@umbraco-cms/backoffice/external/lit';

import './donut-slice.element.js';
import './donut-chart.element.js';

interface ExampleDonutSlice {
	name: string;
	amount: number;
	kind?: string;
	href?: string;
	label?: string;
}

const COLORS = [
	'var(--uui-palette-violet-blue)',
	'var(--uui-palette-jungle-green)',
	'var(--uui-palette-sunglow)',
	'var(--uui-palette-maroon-flush)',
	'var(--uui-palette-malibu)',
	'var(--uui-palette-cocoa-brown)',
	'var(--uui-palette-spanish-pink)',
	'var(--uui-palette-gunmetal)',
];

const renderSlices = (slices: Array<ExampleDonutSlice>) =>
	slices.map(
		(slice, index) =>
			html`<umb-donut-slice
				name=${slice.name}
				amount=${slice.amount}
				kind=${slice.kind ?? 'items'}
				color=${COLORS[index % COLORS.length]}
				href=${ifDefined(slice.href)}
				label=${ifDefined(slice.label)}></umb-donut-slice>`,
	);

const meta: Meta<UmbDonutChartElement> = {
	title: 'Generic Components/Donut chart',
	component: 'umb-donut-chart',
	id: 'umb-donut-chart',
	tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<UmbDonutChartElement>;

export const Default: Story = {
	render: () =>
		html`<umb-donut-chart description="Fruit by colour">
			${renderSlices([
				{ name: 'Red', amount: 10, kind: 'apples' },
				{ name: 'Green', amount: 20, kind: 'apples' },
				{ name: 'Yellow', amount: 10, kind: 'bananas' },
				{ name: 'Purple', amount: 69, kind: 'plums' },
			])}
		</umb-donut-chart>`,
};

export const SingleSlice: Story = {
	render: () =>
		html`<umb-donut-chart description="Only one kind">
			${renderSlices([
				{ name: 'Apples', amount: 42 },
				{ name: 'Pears', amount: 0 },
			])}
		</umb-donut-chart>`,
};

export const TwoEqual: Story = {
	render: () =>
		html`<umb-donut-chart description="Two equal slices">
			${renderSlices([
				{ name: 'Apples', amount: 50 },
				{ name: 'Pears', amount: 50 },
			])}
		</umb-donut-chart>`,
};

export const DominantAndOneTiny: Story = {
	render: () =>
		html`<umb-donut-chart description="One dominant slice and one tiny slice">
			${renderSlices([
				{ name: 'Common', amount: 9999 },
				{ name: 'Rare', amount: 1 },
			])}
		</umb-donut-chart>`,
};

export const DominantAndSeveralTiny: Story = {
	render: () =>
		html`<umb-donut-chart description="Messages by level">
			${renderSlices([
				{ name: 'Information', amount: 50000, kind: 'messages' },
				{ name: 'Warning', amount: 1200, kind: 'messages' },
				{ name: 'Error', amount: 30, kind: 'messages' },
				{ name: 'Fatal', amount: 2, kind: 'messages' },
				{ name: 'Debug', amount: 1, kind: 'messages' },
			])}
		</umb-donut-chart>`,
};

export const ManySlices: Story = {
	render: () =>
		html`<umb-donut-chart description="Twenty-six slices">
			${renderSlices(
				Array.from({ length: 26 }, (_, i) => ({ name: `Slice ${i + 1}`, amount: i === 0 ? 90 : i < 6 ? 2 : 0.4 })),
			)}
		</umb-donut-chart>`,
};

export const Empty: Story = {
	render: () => html`<umb-donut-chart description="Nothing to show"></umb-donut-chart>`,
};

export const ZeroAmounts: Story = {
	render: () =>
		html`<umb-donut-chart description="Nothing counted yet">
			${renderSlices([
				{ name: 'Apples', amount: 0 },
				{ name: 'Pears', amount: 0 },
			])}
		</umb-donut-chart>`,
};

export const WithLinks: Story = {
	render: () =>
		html`<umb-donut-chart description="Fruit with links">
			${renderSlices([
				{ name: 'Apples', amount: 30, href: '#apples', label: 'View apples' },
				{ name: 'Pears', amount: 12, href: '#pears', label: 'View pears' },
				{ name: 'Plums', amount: 3, href: '#plums' },
			])}
		</umb-donut-chart>`,
};

export const LargerSize: Story = {
	render: () =>
		html`<umb-donut-chart
			style="width: 320px"
			svg-size="200"
			radius="96"
			border-size="24"
			min-slice-percent="2"
			description="A larger, thinner ring"
			show-description>
			${renderSlices([
				{ name: 'Apples', amount: 60 },
				{ name: 'Pears', amount: 30 },
				{ name: 'Plums', amount: 9 },
				{ name: 'Figs', amount: 1 },
			])}
		</umb-donut-chart>`,
};
