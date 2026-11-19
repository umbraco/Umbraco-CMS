import type { UmbMbcsSiteCode } from './catalog.js';

export interface UmbMbcsSustainabilityCopy {
	heroSubheadline: string;
	intro: { headline: string; paragraphs: Array<string> };
	showcase: { headline: string; intro: string; materials: Array<string> };
	goals: { headline: string; paragraphs: Array<string>; caption: string };
	closing: { headline: string; paragraphs: Array<string> };
}

const littleOnes: UmbMbcsSustainabilityCopy = {
	heroSubheadline: 'How we try to leave a lighter footprint, one small garment at a time.',
	intro: {
		headline: 'Responsibility, step by step',
		paragraphs: [
			'Children grow quickly, and clothes that are made to be passed on are the most sustainable thing we can offer. That idea shapes how we choose materials, who we work with and how we help our clothes find a second life.',
			'We do not claim to have all the answers. What we can do is set clear goals, be open about where we stand and keep improving. This page explains the choices that matter most.',
		],
	},
	showcase: {
		headline: 'Materials we use',
		intro: 'The fibres and finishes behind our everyday clothes, and why we chose them.',
		materials: [
			'Organic cotton',
			'Merino wool',
			'Bamboo viscose',
			'Recycled polyester',
			'Linen',
			'Waterproof membrane',
		],
	},
	goals: {
		headline: 'Goals we are working towards',
		caption: 'Packing orders without plastic',
		paragraphs: [
			'Our packaging is plastic-free, and we keep working to use less of it. Our take-back programme gives outgrown clothes a second life, either with another family or as new material.',
			'We choose not to take part in Black Friday, because we would rather make fewer, better things than encourage more buying. And our supplier code of conduct is public, so you can see what we expect from the people who make our clothes.',
		],
	},
	closing: {
		headline: 'Hold us to it',
		paragraphs: [
			'Sustainability is a long road, and we expect to be asked questions along the way. If you would like to know more about a material, a supplier or a decision, get in touch. We would rather explain than guess.',
		],
	},
};

const outdoorShop: UmbMbcsSustainabilityCopy = {
	heroSubheadline: 'Clothes that last, repaired when they need it and recycled when they are done.',
	intro: {
		headline: 'Longevity first',
		paragraphs: [
			'The most sustainable garment is the one you keep wearing. That is why we start with durability: sturdy materials, careful construction and a repair service that is part of the business, not an afterthought.',
			'We try to be honest about what we know and what we are still learning. This page explains the choices that matter most, from materials to repair.',
		],
	},
	showcase: {
		headline: 'Materials we use',
		intro: 'The fibres and finishes behind our clothes, and what each one is good at.',
		materials: ['Organic cotton', 'Merino wool', 'Linen', 'Recycled polyester', 'Silk', 'Waterproof membrane'],
	},
	goals: {
		headline: 'Goals we are working towards',
		caption: 'In the repair studio',
		paragraphs: [
			'Our repair studio and lifetime guarantee exist so that clothes keep going. Our take-back programme collects what can no longer be worn, so the materials can be reused.',
			'We publish our supplier code of conduct, we choose not to take part in Black Friday, and we report on our progress, including the places where we still have work to do.',
		],
	},
	closing: {
		headline: 'Hold us to it',
		paragraphs: [
			'If you have a question about a material, a certification or a supplier, ask us. We would rather give you a straight answer than a polished one.',
		],
	},
};

export const getSustainabilityCopy = (site: UmbMbcsSiteCode): UmbMbcsSustainabilityCopy =>
	site === 'LO' ? littleOnes : outdoorShop;
