// Renovate global config that creates a PR to kir-dev/k8s updating the Docker image digest
// https://docs.renovatebot.com/self-hosted-configuration/
// https://docs.renovatebot.com/configuration-options/
module.exports = {
    platform: 'github',
    onboarding: false,
    forkToken: process.env.RENOVATE_TOKEN,
    requireConfig: 'optional',
    gitAuthor: 'Kir-Dev Bot <258595904+kir-dev-bot@users.noreply.github.com>',
    repositories: [
        {
            repository: 'kir-dev/k8s',
            enabledManagers: ['kubernetes'],
            kubernetes: {
                managerFilePatterns: ['place/place.yaml'],
            },
            packageRules: [
                {
                    matchPackageNames: ['*'],
                    enabled: false,
                },
                {
                    matchPackageNames: ['ghcr.io/kir-dev/blace'],
                    enabled: true,
                    pinDigests: true,
                },
            ],
        },
    ],
};
