import type { UmbMbcsSiteCode } from './catalog.js';

export interface UmbMbcsAboutUsCopy {
	lead: { headline: string; paragraphs: Array<string> };
	showcase: { headline: string; intro: string; materials: Array<string> };
	criteria: { headline: string; paragraphs: Array<string>; caption: string };
	closing: { headline: string; paragraphs: Array<string> };
}

const littleOnes: UmbMbcsAboutUsCopy = {
	lead: {
		headline: 'It all starts with the fabric',
		paragraphs: [
			'Good children’s clothes are decided long before the first stitch. Whether a bodysuit stays soft after fifty washes, whether a knee survives a summer of crawling and whether a jacket keeps a child dry on a grey Danish morning all comes down to one thing: what the garment is made of.',
			'That is why we spend more time choosing materials than almost anything else. We test fibres against real life, ask our suppliers hard questions and say no to materials that look good on paper but do not hold up on the playground.',
			'Here are the materials we rely on, and what each one is good at.',
		],
	},
	showcase: {
		headline: 'Quality materials, chosen on purpose',
		intro: 'The fibres and finishes behind our everyday clothes.',
		materials: [
			'Organic cotton',
			'Merino wool',
			'Bamboo viscose',
			'Recycled polyester',
			'Linen',
			'Waterproof membrane',
		],
	},
	criteria: {
		headline: 'How we choose a material',
		caption: 'Checking a new fabric for softness',
		paragraphs: [
			'We ask four questions of every material. Is it gentle on skin? Will it last? Can it be washed and worn again and again? And can we tell you honestly where it comes from?',
			'If the answer to any of them is no, it does not make it into a collection, however good it looks.',
		],
	},
	closing: {
		headline: 'Quality you can feel',
		paragraphs: [
			'You will find the same materials, and the same honesty about them, on every product page. If you want to know more, our Sustainability page explains how we think about responsibility, and Our Story shows where it all began.',
		],
	},
};

const outdoorShop: UmbMbcsAboutUsCopy = {
	lead: {
		headline: 'It all starts with the fabric',
		paragraphs: [
			'A good garment is decided long before the first stitch. Whether a sweater keeps its shape after years of washing, whether a shell keeps you dry on the coast and whether a shirt still looks good after a decade comes down to one thing: what it is made of.',
			'That is why materials are where we spend most of our time. We test fibres against real weather and real wear, ask our suppliers hard questions and say no to materials that look good in a catalogue but do not hold up.',
			'Here are the materials we rely on, and what each one is good at.',
		],
	},
	showcase: {
		headline: 'Quality materials, chosen on purpose',
		intro: 'The fibres and finishes behind our clothes.',
		materials: ['Organic cotton', 'Merino wool', 'Linen', 'Recycled polyester', 'Silk', 'Waterproof membrane'],
	},
	criteria: {
		headline: 'How we choose a material',
		caption: 'Testing a new knit in the studio',
		paragraphs: [
			'We ask four questions of every material. Does it feel good against the skin? Will it last? Can it be repaired and cared for properly? And can we tell you honestly where it comes from?',
			'If the answer to any of them is no, it does not make it into a collection, however good it looks.',
		],
	},
	closing: {
		headline: 'Quality you can feel',
		paragraphs: [
			'You will find the same materials, and the same honesty about them, on every product page. If you want to know more, our Sustainability page explains how we think about responsibility, and Our Story shows where it all began.',
		],
	},
};

export const getAboutUsCopy = (site: UmbMbcsSiteCode): UmbMbcsAboutUsCopy => (site === 'LO' ? littleOnes : outdoorShop);
