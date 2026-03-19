# Changelog

## [0.1.3](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/compare/v0.1.2...v0.1.3) (2026-03-19)


### Bug Fixes

* update PackageProjectUrl to docs site ([e618a68](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/e618a684f0f5dbf96fff28fbb87d47c43025046d))
* update PackageProjectUrl to docs site ([59636fa](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/59636fa6fdc13b1a4eb4ce38fdd0b31c56944c5f))

## [0.1.2](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/compare/v0.1.1...v0.1.2) (2026-03-18)


### Documentation

* anonymise CFE references, add NuGet icon, multi-target net8/9/10 ([a26af0e](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/a26af0e1b94ddf40b7db7b2c18a106d8796166a0))
* restore CSharpFunctionalExtensions name in benchmark comparisons ([11599f9](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/11599f9dade5a522bd8427a15c2c5c89856bbb41))

## [0.1.1](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/compare/v0.1.0...v0.1.1) (2026-03-18)


### Features

* add IResult&lt;T,E&gt; constraint-only interface ([0a9cd7e](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/0a9cd7efbdd762b2726fd292752b87d93993868d))
* add LINQ query syntax extensions (Select, SelectMany) ([7a15e8f](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/7a15e8f84b292182f0628550db505f3d57c11bd2))
* add Map, MapError, Bind extension methods ([efc36c5](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/efc36c584d0b622bb68e26489ead4e29a79bb23a))
* add Maybe&lt;T&gt; zero-alloc Option type ([306d9bb](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/306d9bb2c138be1f703354741ae0ba2a3a46a9e8))
* add non-generic Result pass/fail struct ([1e645a6](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/1e645a664ea9f09a72cd9bf80aef4a06014cd270))
* add Result&lt;T,E&gt; zero-alloc readonly struct ([3aece43](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/3aece433362b3272ff2b6af18a37d9500b724ef6))
* add Result&lt;T&gt; shorthand with string error ([11afe5e](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/11afe5e0a40e3531b6f16ddd42cd648e2637b350))
* add UnitResult&lt;E&gt; zero-alloc struct ([d4ceea9](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/d4ceea97a6f3c27b86c312e59d346a1b8192a66f))
* add ValueTask async extension methods ([eb21883](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/eb218837c1d3a55b129ef6e0a761053e6d129d4d))


### Documentation

* add CFE head-to-head benchmark results ([ccad291](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/ccad29129cc3a546cfd074ab30f11318ce767d11))
* add CFE head-to-head benchmark results ([fd459b7](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/fd459b7e28856e018b1f2be82a8d127a029f3be4))
* add implementation plan for ZeroAlloc.Results ([f82e580](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/f82e5801dda455d9985345c06fe408819210f65f))
* add README, GitHub workflows, release config, and Docusaurus docs ([8a4496c](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/8a4496c9402247eb10dc62c4646932bb76396055))
* remove direct CFE references from public docs ([acabaf2](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/acabaf20267aabf4302e68a11c24247e44625f23))
* restore benchmark results, anonymise comparison library name ([0d0de27](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/0d0de2712a6cc6fe56a3efd2e0a4d3eafe0b62ad))
* update benchmark numbers from verified artifact results ([f5118fd](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/f5118fd52d90992761d0b9f004e388d00b9c1e0a))


### Tests

* add BenchmarkDotNet allocation benchmarks — Gen0 must be 0 ([4f9cc10](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/4f9cc105d7c137b30fd01bbe5f7f645a0d5703b9))
* add Ensure, Combine tests ([4b2cbf3](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/4b2cbf344f5e71a108e321782fe90163c32a0f9d))
* add Match, Tap, TapError tests ([749cff3](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/749cff386f12e0f4516b3e92b19cf55887ffa473))
* fill coverage gaps — ToString, implicit ops, edge cases, async failure paths ([35368ae](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/commit/35368aeffdf400ead8aea77e85db48336b9cc8d2))
